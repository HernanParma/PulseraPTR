using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PulseraPTR.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardApiController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenDto>> Resumen(CancellationToken ct = default) =>
        Ok(await _dashboard.ObtenerResumenAsync(ct));

    [HttpGet("paciente/{pacienteId:int}")]
    public async Task<ActionResult<PacienteDetalleDto>> Paciente(int pacienteId, CancellationToken ct = default) =>
        Ok(await _dashboard.ObtenerDetallePacienteAsync(pacienteId, ct));
}
