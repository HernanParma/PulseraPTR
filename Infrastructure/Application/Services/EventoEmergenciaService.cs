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

public class EventoEmergenciaService : IEventoEmergenciaService
{
    private readonly IEventoEmergenciaRepository _eventos;
    private readonly IPacienteRepository _pacientes;
    private readonly IAlertaRepository _alertas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPulseraRealtimeNotifier _notifier;
    private readonly INotificacionContactoEmergencia _contactoEmergencia;
    private readonly IConfiguration _configuration;

    public EventoEmergenciaService(
        IEventoEmergenciaRepository eventos,
        IPacienteRepository pacientes,
        IAlertaRepository alertas,
        IUnitOfWork unitOfWork,
        IPulseraRealtimeNotifier notifier,
        INotificacionContactoEmergencia contactoEmergencia,
        IConfiguration configuration)
    {
        _eventos = eventos;
        _pacientes = pacientes;
        _alertas = alertas;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
        _contactoEmergencia = contactoEmergencia;
        _configuration = configuration;
    }

    public async Task<EventoEmergenciaDto> RegistrarSosAsync(CrearEventoEmergenciaDto dto, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacientes.GetByIdAsync(dto.PacienteId, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el paciente {dto.PacienteId}.");

        var tipo = dto.TipoEvento;
        if (tipo != TipoEventoEmergencia.SOS)
            tipo = TipoEventoEmergencia.SOS;

        var estado = EstadoClinico.CRITICO;
        var mensaje = string.IsNullOrWhiteSpace(dto.Mensaje) ? "Emergencia SOS" : dto.Mensaje.Trim();

        var evento = new EventoEmergencia
        {
            PacienteId = dto.PacienteId,
            FechaHora = dto.FechaHora,
            TipoEvento = tipo,
            Estado = estado,
            Mensaje = mensaje,
            Atendido = false
        };

        await _eventos.AddAsync(evento, cancellationToken);

        var alerta = new Alerta
        {
            PacienteId = dto.PacienteId,
            FechaHora = dto.FechaHora,
            TipoAlerta = TipoAlerta.SosManual,
            Estado = estado,
            Mensaje = mensaje,
            Leida = false
        };
        await _alertas.AddAsync(alerta, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        evento.Paciente = paciente;
        alerta.Paciente = paciente;

        var dtoEvento = evento.ToDto();
        await _notifier.NotificarNuevoEventoSosAsync(dtoEvento, cancellationToken);
        await _notifier.NotificarNuevaAlertaAsync(alerta.ToDto(), cancellationToken);

        if (_configuration.GetValue("Pulsera:NotificarContactoEmergencia", true))
        {
            await _contactoEmergencia.EnviarAvisoAsync(
                paciente.Id,
                paciente.Nombre,
                paciente.ContactoEmergencia,
                "Emergencia SOS — PulseraPTR",
                $"{paciente.Nombre}: {mensaje}",
                cancellationToken);
        }

        return dtoEvento;
    }

    public async Task<IReadOnlyList<EventoEmergenciaDto>> ListarAsync(int? pacienteId, bool? atendido, CancellationToken cancellationToken = default)
    {
        var items = await _eventos.BuscarAsync(pacienteId, atendido, TipoEventoEmergencia.SOS, cancellationToken);
        return items.OrderByDescending(e => e.FechaHora).Select(e => e.ToDto()).ToList();
    }

    public async Task MarcarAtendidoAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _eventos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el evento {id}.");

        entity.Atendido = true;
        _eventos.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
