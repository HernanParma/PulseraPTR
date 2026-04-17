using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers;

public class PacientesController : Controller
{
    private readonly IPacienteService _pacientes;

    public PacientesController(IPacienteService pacientes)
    {
        _pacientes = pacientes;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var lista = await _pacientes.ListarAsync(incluirInactivos: true, cancellationToken);
        return View(lista);
    }

    public IActionResult Create() => View(new CrearPacienteDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearPacienteDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _pacientes.CrearAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var p = await _pacientes.ObtenerPorIdAsync(id, cancellationToken);
        if (p is null)
            return NotFound();

        var model = new ActualizarPacienteDto
        {
            Nombre = p.Nombre,
            Edad = p.Edad,
            Dni = p.Dni,
            ContactoEmergencia = p.ContactoEmergencia,
            Observaciones = p.Observaciones
        };

        ViewBag.Id = id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActualizarPacienteDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View(model);
        }

        await _pacientes.ActualizarAsync(id, model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var detalle = await _pacientes.ObtenerDetalleAsync(id, cancellationToken);
            return View(detalle);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _pacientes.DarDeBajaLogicaAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
