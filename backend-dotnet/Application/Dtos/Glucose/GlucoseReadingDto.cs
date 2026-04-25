using Domain.Enums;

namespace Application.Dtos.Glucose;

public sealed class GlucoseReadingDto
{
    public int Id { get; init; }
    public int PacienteId { get; init; }
    public DateTime ReadingDateTimeUtc { get; init; }
    public string DateRaw { get; init; } = string.Empty;
    public string TimeRaw { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int GlucoseMgDl { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string? SourceFileName { get; init; }
    public GlucoseReadingSource Source { get; init; }
    public GlucoseRangeBand Band { get; init; }
}
