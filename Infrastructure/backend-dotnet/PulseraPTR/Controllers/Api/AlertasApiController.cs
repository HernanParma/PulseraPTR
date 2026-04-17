using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/alertas")]
public class AlertasApiController : ControllerBase
{
    private readonly IAlertaService _alertas;

    public AlertasApiController(IAlertaService alertas)
    {
        _alertas = alertas;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertaDto>>> Get(
        [FromQuery] int? pacienteId,
        [FromQuery] bool? leida,
        CancellationToken ct = default) =>
        Ok(await _alertas.ListarAsync(pacienteId, leida, ct));

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<ActionResult<IReadOnlyList<AlertaDto>>> GetPorPaciente(int pacienteId, CancellationToken ct = default) =>
        Ok(await _alertas.ListarAsync(pacienteId, null, ct));

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken ct = default)
    {
        await _alertas.MarcarLeidaAsync(id, ct);
        return NoContent();
    }
}
