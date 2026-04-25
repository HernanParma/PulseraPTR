using Application.Dtos.Glucose;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/glucose")]
public sealed class GlucoseApiController : ControllerBase
{
    private readonly IGlucoseImportService _import;
    private readonly IGlucoseReadingsQueryService _query;

    public GlucoseApiController(IGlucoseImportService import, IGlucoseReadingsQueryService query)
    {
        _import = import;
        _query = query;
    }

    /// <summary>
    /// Importa un CSV exportado desde mySugr.
    /// multipart/form-data: campo <c>file</c>. Query: <c>patientId</c> (mismo valor que <c>PacienteId</c> en BD).
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(GlucoseImportResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GlucoseImportResultDto>> Import(
        [FromQuery(Name = "patientId")] int patientId,
        [FromForm(Name = "file")] IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Debe adjuntar un archivo CSV (campo multipart \"file\").");

        await using var stream = file.OpenReadStream();
        var result = await _import.ImportMySugrCsvAsync(patientId, stream, file.FileName, cancellationToken);
        return Ok(result);
    }

    [HttpGet("dashboard/{patientId:int}")]
    [ProducesResponseType(typeof(GlucoseDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GlucoseDashboardDto>> Dashboard(int patientId, CancellationToken cancellationToken = default) =>
        Ok(await _query.GetDashboardAsync(patientId, cancellationToken));

    [HttpGet("readings/{patientId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<GlucoseReadingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GlucoseReadingDto>>> Readings(int patientId, CancellationToken cancellationToken = default) =>
        Ok(await _query.GetReadingsAsync(patientId, cancellationToken));

    [HttpGet("alerts/{patientId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<GlucoseAlertItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GlucoseAlertItemDto>>> Alerts(int patientId, CancellationToken cancellationToken = default) =>
        Ok(await _query.GetAlertsAsync(patientId, cancellationToken));
}
