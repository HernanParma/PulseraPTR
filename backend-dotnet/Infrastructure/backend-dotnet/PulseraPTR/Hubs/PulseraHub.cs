using Microsoft.AspNetCore.SignalR;

namespace PulseraPTR.Hubs;

public class PulseraHub : Hub
{
    /// <summary>
    /// Desde el dashboard web: unir la conexión al grupo de difusión.
    /// </summary>
    public Task JoinDashboard()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
    }

    /// <summary>
    /// Vista Glucemia por paciente: recibe el evento glucemiaActualizada tras importar CSV/email.
    /// </summary>
    public Task JoinGlucoseDashboard(int pacienteId)
    {
        if (pacienteId <= 0)
            return Task.CompletedTask;

        return Groups.AddToGroupAsync(Context.ConnectionId, GlucoseDashboardGroupName(pacienteId));
    }

    internal static string GlucoseDashboardGroupName(int pacienteId) => $"glucose-{pacienteId}";
}
