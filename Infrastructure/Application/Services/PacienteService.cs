using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mapping;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacientes;
    private readonly IMedicionRepository _mediciones;
    private readonly IAlertaRepository _alertas;
    private readonly IEventoEmergenciaRepository _eventos;
    private readonly IClasificacionEstadoCardiaco _clasificacion;
    private readonly IUnitOfWork _unitOfWork;

    public PacienteService(
        IPacienteRepository pacientes,
        IMedicionRepository mediciones,
        IAlertaRepository alertas,
        IEventoEmergenciaRepository eventos,
        IClasificacionEstadoCardiaco clasificacion,
        IUnitOfWork unitOfWork)
    {
        _pacientes = pacientes;
        _mediciones = mediciones;
        _alertas = alertas;
        _eventos = eventos;
        _clasificacion = clasificacion;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PacienteDto>> ListarAsync(bool incluirInactivos, CancellationToken cancellationToken = default)
    {
        var lista = await _pacientes.GetAllAsync(incluirInactivos, cancellationToken);
        var resultado = new List<PacienteDto>();

        foreach (var p in lista)
        {
            var ultima = await _mediciones.GetUltimaPorPacienteAsync(p.Id, cancellationToken);
            var estado = ultima != null ? ultima.Estado : (EstadoClinico?)null;
            resultado.Add(p.ToDto(estado, ultima?.FechaHora));
        }

        return resultado;
    }

    public async Task<PacienteDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _pacientes.GetByIdAsync(id, cancellationToken);
        if (p is null)
            return null;

        var ultima = await _mediciones.GetUltimaPorPacienteAsync(id, cancellationToken);
        return p.ToDto(ultima?.Estado, ultima?.FechaHora);
    }

    public async Task<PacienteDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _pacientes.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el paciente {id}.");

        var mediciones = await _mediciones.BuscarAsync(id, null, null, null, null, cancellationToken);
        var alertas = await _alertas.BuscarAsync(id, null, cancellationToken);
        var eventos = await _eventos.BuscarAsync(id, null, null, cancellationToken);

        var ultima = mediciones.OrderByDescending(m => m.FechaHora).FirstOrDefault();

        return new PacienteDetalleDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Edad = p.Edad,
            Dni = p.Dni,
            ContactoEmergencia = p.ContactoEmergencia,
            Observaciones = p.Observaciones,
            Activo = p.Activo,
            EstadoActual = ultima?.Estado,
            UltimaMedicionFechaHora = ultima?.FechaHora,
            Mediciones = mediciones.OrderByDescending(m => m.FechaHora).Select(m => m.ToDto()).ToList(),
            Alertas = alertas.OrderByDescending(a => a.FechaHora).Select(a => a.ToDto()).ToList(),
            EventosSos = eventos.OrderByDescending(e => e.FechaHora).Select(e => e.ToDto()).ToList()
        };
    }

    public async Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Paciente
        {
            Nombre = dto.Nombre.Trim(),
            Edad = dto.Edad,
            Dni = string.IsNullOrWhiteSpace(dto.Dni) ? null : dto.Dni.Trim(),
            ContactoEmergencia = dto.ContactoEmergencia.Trim(),
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
            Activo = true
        };

        await _pacientes.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _pacientes.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el paciente {id}.");

        entity.Nombre = dto.Nombre.Trim();
        entity.Edad = dto.Edad;
        entity.Dni = string.IsNullOrWhiteSpace(dto.Dni) ? null : dto.Dni.Trim();
        entity.ContactoEmergencia = dto.ContactoEmergencia.Trim();
        entity.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim();

        _pacientes.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ultima = await _mediciones.GetUltimaPorPacienteAsync(id, cancellationToken);
        return entity.ToDto(ultima?.Estado, ultima?.FechaHora);
    }

    public async Task DarDeBajaLogicaAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _pacientes.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el paciente {id}.");

        entity.Activo = false;
        _pacientes.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
