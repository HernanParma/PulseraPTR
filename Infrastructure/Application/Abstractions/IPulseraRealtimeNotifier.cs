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
}
