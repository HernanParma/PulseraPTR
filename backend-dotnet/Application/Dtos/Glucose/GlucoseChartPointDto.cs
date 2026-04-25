namespace Application.Dtos.Glucose;

public sealed class GlucoseChartPointDto
{
    public DateTime AtUtc { get; init; }
    public int GlucoseMgDl { get; init; }
    public string Label { get; init; } = string.Empty;
}
