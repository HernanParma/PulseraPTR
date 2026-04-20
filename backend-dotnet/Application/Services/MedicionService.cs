using Application.Abstractions;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mapping;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class MedicionService : IMedicionService
{
    private readonly IMedicionRepository _mediciones;
    private readonly IPacienteRepository _pacientes;
    private readonly IAlertaRepository _alertas;
    private readonly IClasificacionEstadoCardiaco _clasificacion;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPulseraRealtimeNotifier _notifier;
    private readonly INotificacionContactoEmergencia _contactoEmergencia;
    private readonly IConfiguration _configuration;

    public MedicionService(
        IMedicionRepository mediciones,
        IPacienteRepository pacientes,
        IAlertaRepository alertas,
        IClasificacionEstadoCardiaco clasificacion,
        IUnitOfWork unitOfWork,
        IPulseraRealtimeNotifier notifier,
        INotificacionContactoEmergencia contactoEmergencia,
        IConfiguration configuration)
    {
        _mediciones = mediciones;
        _pacientes = pacientes;
        _alertas = alertas;
        _clasificacion = clasificacion;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
        _contactoEmergencia = contactoEmergencia;
        _configuration = configuration;
    }

    public async Task<MedicionDto> RegistrarAsync(CrearMedicionDto dto, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacientes.GetByIdAsync(dto.PacienteId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el paciente {dto.PacienteId}.");

        if (!paciente.Activo)
            throw new InvalidOperationException("El paciente no está activo.");

        var estadoCalculado = _clasificacion.CalcularPorFrecuenciaCardiaca(dto.FrecuenciaCardiaca);
        var fuera = _clasificacion.EsFueraDeRango(estadoCalculado);

        var mensaje = string.IsNullOrWhiteSpace(dto.MensajeAlerta)
            ? _clasificacion.ObtenerMensajeSugerido(estadoCalculado, dto.FrecuenciaCardiaca)
            : dto.MensajeAlerta.Trim();

        var entity = new Medicion
        {
            PacienteId = dto.PacienteId,
            FechaHora = dto.FechaHora,
            ValorMedicion = dto.FrecuenciaCardiaca,
            Estado = estadoCalculado,
            MensajeAlerta = mensaje,
            OrigenDato = string.IsNullOrWhiteSpace(dto.OrigenDato) ? "Desconocido" : dto.OrigenDato.Trim(),
            EsFueraDeRango = fuera
        };

        await _mediciones.AddAsync(entity, cancellationToken);

        Alerta? alertaGenerada = null;
        if (fuera)
        {
            alertaGenerada = new Alerta
            {
                PacienteId = dto.PacienteId,
                FechaHora = dto.FechaHora,
                TipoAlerta = TipoAlerta.FrecuenciaCardiaca,
                Estado = estadoCalculado,
                Mensaje = mensaje,
                Leida = false
            };
            await _alertas.AddAsync(alertaGenerada, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entity.Paciente = paciente;
        var medicionDto = entity.ToDto();

        await _notifier.NotificarNuevaMedicionAsync(medicionDto, cancellationToken);

        if (alertaGenerada is not null)
        {
            alertaGenerada.Paciente = paciente;
            await _notifier.NotificarNuevaAlertaAsync(alertaGenerada.ToDto(), cancellationToken);

            await NotificarContactoSiCorrespondeAsync(
                paciente,
                estadoCalculado,
                mensaje,
                dto.FrecuenciaCardiaca,
                cancellationToken);
        }

        return medicionDto;
    }

    public async Task<IReadOnlyList<MedicionDto>> ListarAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        CancellationToken cancellationToken = default)
    {
        var items = await _mediciones.BuscarAsync(pacienteId, fechaDesde, fechaHasta, estado, null, cancellationToken);
        return items.OrderByDescending(m => m.FechaHora).Select(m => m.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<MedicionDto>> ListarFueraDeRangoAsync(CancellationToken cancellationToken = default)
    {
        var items = await _mediciones.BuscarAsync(null, null, null, null, true, cancellationToken);
        return items.OrderByDescending(m => m.FechaHora).Select(m => m.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<MedicionDto>> ListarPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        var items = await _mediciones.BuscarAsync(pacienteId, null, null, null, null, cancellationToken);
        return items.OrderByDescending(m => m.FechaHora).Select(m => m.ToDto()).ToList();
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _mediciones.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la medición {id}.");

        _mediciones.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task NotificarContactoSiCorrespondeAsync(
        Paciente paciente,
        EstadoClinico estado,
        string mensaje,
        int frecuenciaCardiaca,
        CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue("Pulsera:NotificarContactoEmergencia", true))
            return;

        var soloCritico = _configuration.GetValue("Pulsera:NotificarContactoSoloCriticoEnFc", false);
        if (soloCritico && estado != EstadoClinico.CRITICO)
            return;

        await _contactoEmergencia.EnviarAvisoAsync(
            paciente.Id,
            paciente.Nombre,
            paciente.ContactoEmergencia,
            "Alerta PulseraPTR — frecuencia cardíaca",
            $"{paciente.Nombre}: {mensaje} (FC {frecuenciaCardiaca} lpm, estado {estado}).",
            cancellationToken);
    }
}
