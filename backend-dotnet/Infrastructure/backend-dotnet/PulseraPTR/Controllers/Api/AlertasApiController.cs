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

    [HttpGet("contador-sin-leer")]
    public async Task<ActionResult<object>> ContadorSinLeer(CancellationToken ct = default) =>
        Ok(new { count = await _alertas.ContarSinLeerAsync(ct) });

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertaDto>>> Get(
        [FromQuery] int? pacienteId,
        [FromQuery] bool? leida,
        CancellationToken ct = default) =>
        Ok(await _alertas.ListarAsync(pacienteId, leida, ct));

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<ActionResult<IReadOnlyList<AlertaDto>>> GetPorPaciente(int pacienteId, CancellationToken ct = default) =>
        Ok(await _alertas.ListarAsync(pacienteId, null, ct));

    [HttpGet("recientes")]
    public async Task<ActionResult<IReadOnlyList<AlertaDto>>> Recientes(
        [FromQuery] int cantidad = 12,
        CancellationToken ct = default) =>
        Ok(await _alertas.ListarRecientesAsync(cantidad, ct));

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken ct = default)
    {
        await _alertas.MarcarLeidaAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
    {
        await _alertas.EliminarAsync(id, ct);
        return NoContent();
    }
}
