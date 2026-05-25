using Application.Dtos.Glucose;
using Domain.Entities;

namespace Application.Interfaces;

public interface IGlucoseAlertEvaluator
{
    IReadOnlyList<GlucoseAlertItemDto> BuildAlerts(IReadOnlyList<GlucoseReading> readingsInSummaryWindow);
}
