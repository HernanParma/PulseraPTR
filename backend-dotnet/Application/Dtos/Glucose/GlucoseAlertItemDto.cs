namespace Application.Dtos.Glucose;

public sealed class GlucoseAlertItemDto
{
    public string Code { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime OccurredAtUtc { get; init; }
    public int? RelatedReadingId { get; init; }
}
