using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/pacientes")]
public class PacientesApiController : ControllerBase
{
    private readonly IPacienteService _pacientes;

    public PacientesApiController(IPacienteService pacientes)
    {
        _pacientes = pacientes;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PacienteDto>>> Get([FromQuery] bool incluirInactivos = false, CancellationToken ct = default) =>
        Ok(await _pacientes.ListarAsync(incluirInactivos, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PacienteDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _pacientes.ObtenerPorIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Post([FromBody] CrearPacienteDto dto, CancellationToken ct = default)
    {
        var creado = await _pacientes.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Put(int id, [FromBody] ActualizarPacienteDto dto, CancellationToken ct = default) =>
        Ok(await _pacientes.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _pacientes.DarDeBajaLogicaAsync(id, ct);
        return NoContent();
    }
}
