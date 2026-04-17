using Application.Dtos;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync(CancellationToken cancellationToken = default);
    Task<PacienteDetalleDto> ObtenerDetallePacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
}
