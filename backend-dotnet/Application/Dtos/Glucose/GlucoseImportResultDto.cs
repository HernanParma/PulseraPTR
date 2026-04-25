namespace Application.Dtos.Glucose;

public sealed class GlucoseImportResultDto
{
    public int RowsRead { get; init; }
    public int Imported { get; init; }
    public int Duplicates { get; init; }
    public int Discarded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
