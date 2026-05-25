using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.SignalR;
using PulseraPTR.Hubs;

namespace PulseraPTR.Services;

public class PulseraRealtimeNotifier : IPulseraRealtimeNotifier
{
    private readonly IHubContext<PulseraHub> _hubContext;

    public PulseraRealtimeNotifier(IHubContext<PulseraHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotificarNuevaMedicionAsync(MedicionDto medicion, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group("dashboard").SendAsync("nuevaMedicion", medicion, cancellationToken);

    public Task NotificarNuevaAlertaAsync(AlertaDto alerta, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group("dashboard").SendAsync("nuevaAlerta", alerta, cancellationToken);

    public Task NotificarNuevoEventoSosAsync(EventoEmergenciaDto evento, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group("dashboard").SendAsync("nuevoEventoSos", evento, cancellationToken);

    public Task NotificarGlucemiaActualizadaAsync(int pacienteId, CancellationToken cancellationToken = default) =>
        _hubContext.Clients
            .Group(PulseraHub.GlucoseDashboardGroupName(pacienteId))
            .SendAsync("glucemiaActualizada", new { pacienteId }, cancellationToken);
}
