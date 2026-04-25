using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers;

public sealed class GlucoseController : Controller
{
    private readonly IGlucoseImportService _import;
    private readonly IGlucoseReadingsQueryService _query;
    private readonly IPacienteService _pacientes;

    public GlucoseController(
        IGlucoseImportService import,
        IGlucoseReadingsQueryService query,
        IPacienteService pacientes)
    {
        _import = import;
        _query = query;
        _pacientes = pacientes;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(int id, CancellationToken cancellationToken)
    {
        var p = await _pacientes.ObtenerPorIdAsync(id, cancellationToken);
        if (p is null)
            return NotFound();

        ViewBag.PacienteNombre = p.Nombre;
        var dto = await _query.GetDashboardAsync(id, cancellationToken);
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        ViewBag.Pacientes = await _pacientes.ListarAsync(incluirInactivos: false, cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Import(
        [FromForm(Name = "file")] IFormFile file,
        [FromForm] int pacienteId,
        CancellationToken cancellationToken)
    {
        ViewBag.Pacientes = await _pacientes.ListarAsync(incluirInactivos: false, cancellationToken);

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Seleccioná un archivo CSV (campo \"file\").");
            return View();
        }

        await using var stream = file.OpenReadStream();
        var result = await _import.ImportMySugrCsvAsync(pacienteId, stream, file.FileName, cancellationToken);
        ViewBag.ImportResult = result;

        return View();
    }
}
