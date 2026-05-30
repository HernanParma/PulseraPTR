using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.ViewComponents;

public sealed class AlertasNavbarBadgeViewComponent : ViewComponent
{
    private readonly IAlertaService _alertas;

    public AlertasNavbarBadgeViewComponent(IAlertaService alertas)
    {
        _alertas = alertas;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _alertas.ContarSinLeerAsync(HttpContext.RequestAborted);
        return View(count);
    }
}
