using Domain.Enums;

namespace Domain.Entities;

public class GlucoseReading
{
    public int Id { get; set; }

    /// <summary>
    /// Paciente asociado (FK a Pacientes.Id).
    /// </summary>
    public int PacienteId { get; set; }

    /// <summary>
    /// Valor de fecha/hora en UTC (combinación fecha+hora CSV + zona horaria). Kind no persistido en SQL Server.
    /// </summary>
    public DateTime ReadingDateTime { get; set; }

    public string DateRaw { get; set; } = string.Empty;
    public string TimeRaw { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int GlucoseMgDl { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string? SourceFileName { get; set; }
    public GlucoseReadingSource Source { get; set; } = GlucoseReadingSource.MySugrCsvImport;

    /// <summary>
    /// Hash determinístico para deduplicar reimportaciones (paciente + instante + valor + etiqueta).
    /// </summary>
    public string ImportHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Paciente Paciente { get; set; } = null!;
}
