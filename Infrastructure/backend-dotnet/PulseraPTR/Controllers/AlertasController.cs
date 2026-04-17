using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers;

public class AlertasController : Controller
{
    private readonly IAlertaService _alertas;
    private readonly IPacienteService _pacientes;

    public AlertasController(IAlertaService alertas, IPacienteService pacientes)
    {
        _alertas = alertas;
        _pacientes = pacientes;
    }

    public async Task<IActionResult> Index(int? pacienteId, bool? leida, CancellationToken cancellationToken)
    {
        ViewBag.Pacientes = await _pacientes.ListarAsync(incluirInactivos: false, cancellationToken);
        ViewBag.PacienteId = pacienteId;
        ViewBag.Leida = leida;

        var lista = await _alertas.ListarAsync(pacienteId, leida, cancellationToken);
        return View(lista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken cancellationToken)
    {
        await _alertas.MarcarLeidaAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
