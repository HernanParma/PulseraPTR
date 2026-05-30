using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers;

public class MedicionesController : Controller
{
    private readonly IMedicionService _mediciones;
    private readonly IMedicionesListadoService _listado;
    private readonly IPacienteService _pacientes;

    public MedicionesController(
        IMedicionService mediciones,
        IMedicionesListadoService listado,
        IPacienteService pacientes)
    {
        _mediciones = mediciones;
        _listado = listado;
        _pacientes = pacientes;
    }

    public async Task<IActionResult> Index(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        ViewBag.Pacientes = await _pacientes.ListarAsync(incluirInactivos: false, cancellationToken);
        ViewBag.PacienteId = pacienteId;
        ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.Estado = estado;
        ViewBag.Pagina = pagina;

        var index = await _listado.ObtenerIndexAsync(
            pacienteId, fechaDesde, fechaHasta, estado, pagina, cancellationToken: cancellationToken);
        return View(index);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        int? returnPacienteDetalleId,
        int? pacienteId,
        string? fechaDesde,
        string? fechaHasta,
        EstadoClinico? estado,
        int? pagina,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediciones.EliminarAsync(id, cancellationToken);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        if (returnPacienteDetalleId.HasValue)
            return RedirectToAction("Details", "Pacientes", new { id = returnPacienteDetalleId.Value });

        return RedirectToAction(nameof(Index), new { pacienteId, fechaDesde, fechaHasta, estado, pagina });
    }
}
