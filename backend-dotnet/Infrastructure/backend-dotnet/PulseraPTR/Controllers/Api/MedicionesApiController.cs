using Application.Dtos;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/mediciones")]
public class MedicionesApiController : ControllerBase
{
    private readonly IMedicionService _mediciones;

    public MedicionesApiController(IMedicionService mediciones)
    {
        _mediciones = mediciones;
    }

    [HttpPost]
    public async Task<ActionResult<MedicionDto>> Post([FromBody] CrearMedicionDto dto, CancellationToken ct = default) =>
        Ok(await _mediciones.RegistrarAsync(dto, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicionDto>>> Get(
        [FromQuery] int? pacienteId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] EstadoClinico? estado,
        CancellationToken ct = default) =>
        Ok(await _mediciones.ListarAsync(pacienteId, fechaDesde, fechaHasta, estado, ct));

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<ActionResult<IReadOnlyList<MedicionDto>>> GetPorPaciente(int pacienteId, CancellationToken ct = default) =>
        Ok(await _mediciones.ListarPorPacienteAsync(pacienteId, ct));

    [HttpGet("fuera-de-rango")]
    public async Task<ActionResult<IReadOnlyList<MedicionDto>>> GetFueraDeRango(CancellationToken ct = default) =>
        Ok(await _mediciones.ListarFueraDeRangoAsync(ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediciones.EliminarAsync(id, ct);
        return NoContent();
    }
}
