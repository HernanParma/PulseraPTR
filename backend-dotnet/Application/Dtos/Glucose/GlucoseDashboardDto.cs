namespace Application.Dtos.Glucose;

public sealed class GlucoseDashboardDto
{
    public int PacienteId { get; init; }

    public GlucoseReadingDto? LastReading { get; init; }
    public double? AverageMgDl { get; init; }
    public int? MinMgDl { get; init; }
    public int? MaxMgDl { get; init; }

    public int HypoglycemiaCount { get; init; }
    public int HyperglycemiaCount { get; init; }
    public int HighlightedAlertCount { get; init; }

    public IReadOnlyList<GlucoseReadingDto> RecentReadings { get; init; } = Array.Empty<GlucoseReadingDto>();
    public IReadOnlyList<GlucoseAlertItemDto> Alerts { get; init; } = Array.Empty<GlucoseAlertItemDto>();
    public IReadOnlyList<GlucoseChartPointDto> ChartPoints { get; init; } = Array.Empty<GlucoseChartPointDto>();

    public DateTime SummaryFromUtc { get; init; }
    public DateTime SummaryToUtc { get; init; }
}
