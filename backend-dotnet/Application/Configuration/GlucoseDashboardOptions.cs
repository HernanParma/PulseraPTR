namespace Application.Configuration;

/// <summary>
/// Ventanas de tiempo para estadísticas y gráfico (appsettings: GlucoseDashboard).
/// </summary>
public sealed class GlucoseDashboardOptions
{
    public const string SectionName = "GlucoseDashboard";

    /// <summary>Días hacia atrás para promedios, min/max e hipos/hiper en resumen.</summary>
    public int SummaryWindowDays { get; set; } = 30;

    /// <summary>Puntos máximos en serie temporal (muestreo uniforme si hay más lecturas).</summary>
    public int ChartMaxPoints { get; set; } = 60;

    /// <summary>Filas en tabla de lecturas recientes.</summary>
    public int RecentReadingsTake { get; set; } = 25;
}
