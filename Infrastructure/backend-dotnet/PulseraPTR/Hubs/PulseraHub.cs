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
}
