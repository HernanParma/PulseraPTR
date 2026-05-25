using Application.Dtos;

namespace Application.Abstractions;

/// <summary>
/// Contrato para notificar al dashboard en tiempo real (implementación en la capa de presentación).
/// </summary>
public interface IPulseraRealtimeNotifier
{
    Task NotificarNuevaMedicionAsync(MedicionDto medicion, CancellationToken cancellationToken = default);
    Task NotificarNuevaAlertaAsync(AlertaDto alerta, CancellationToken cancellationToken = default);
    Task NotificarNuevoEventoSosAsync(EventoEmergenciaDto evento, CancellationToken cancellationToken = default);

    /// <summary>
    /// Avisar al dashboard de glucemia del paciente (vista web) que hay datos nuevos tras importación CSV/email.
    /// </summary>
    Task NotificarGlucemiaActualizadaAsync(int pacienteId, CancellationToken cancellationToken = default);
}
