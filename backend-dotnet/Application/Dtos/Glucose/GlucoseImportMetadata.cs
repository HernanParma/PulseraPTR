namespace Application.Dtos.Glucose;

/// <summary>
/// Metadatos de un lote de importación (mail IMAP o carga manual).
/// </summary>
public sealed class GlucoseImportMetadata
{
    public string? ImportBatchId { get; init; }
    public DateTime? EmailReceivedAtUtc { get; init; }
}
