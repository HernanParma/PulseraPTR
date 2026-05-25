using Application.Dtos.Glucose;

namespace Application.Interfaces;

public interface IGlucoseReadingsQueryService
{
    Task<GlucoseDashboardDto> GetDashboardAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseReadingDto>> GetReadingsAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseAlertItemDto>> GetAlertsAsync(int pacienteId, CancellationToken cancellationToken = default);
}
