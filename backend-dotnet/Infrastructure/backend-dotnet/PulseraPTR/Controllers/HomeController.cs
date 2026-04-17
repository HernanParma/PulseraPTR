using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardService _dashboard;

    public HomeController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var resumen = await _dashboard.ObtenerResumenAsync(cancellationToken);
        return View(resumen);
    }
}
