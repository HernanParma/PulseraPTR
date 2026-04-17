using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/eventos")]
public class EventosApiController : ControllerBase
{
    private readonly IEventoEmergenciaService _eventos;

    public EventosApiController(IEventoEmergenciaService eventos)
    {
        _eventos = eventos;
    }

    [HttpPost("sos")]
    public async Task<ActionResult<EventoEmergenciaDto>> PostSos([FromBody] CrearEventoEmergenciaDto dto, CancellationToken ct = default) =>
        Ok(await _eventos.RegistrarSosAsync(dto, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventoEmergenciaDto>>> Get(
        [FromQuery] int? pacienteId,
        [FromQuery] bool? atendido,
        CancellationToken ct = default) =>
        Ok(await _eventos.ListarAsync(pacienteId, atendido, ct));

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<ActionResult<IReadOnlyList<EventoEmergenciaDto>>> GetPorPaciente(int pacienteId, CancellationToken ct = default) =>
        Ok(await _eventos.ListarAsync(pacienteId, null, ct));

    [HttpPut("{id:int}/atender")]
    public async Task<IActionResult> Atender(int id, CancellationToken ct = default)
    {
        await _eventos.MarcarAtendidoAsync(id, ct);
        return NoContent();
    }
}
