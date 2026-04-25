namespace Application.Configuration;

/// <summary>
/// Umbrales y ventanas para reglas de glucemia (appsettings: GlucoseAlerts).
/// Valores por defecto alineados con guías habituales; ajustables sin recompilar lógica dispersa.
/// </summary>
public sealed class GlucoseAlertOptions
{
    public const string SectionName = "GlucoseAlerts";

    public int SevereHypoglycemiaMaxExclusive { get; set; } = 54;
    public int HypoglycemiaMaxExclusive { get; set; } = 70;
    public int HyperglycemiaMinExclusive { get; set; } = 180;
    public int SevereHyperglycemiaMinExclusive { get; set; } = 250;
    public int CriticalIsolatedMinExclusive { get; set; } = 300;

    /// <summary>Ventana en días para reglas de ayunas / post-comida.</summary>
    public int RollingDays { get; set; } = 7;

    public int FastingHighThresholdExclusive { get; set; } = 130;
    public int PostMealHighThresholdExclusive { get; set; } = 180;

    /// <summary>Mínimo de lecturas que disparan alerta agregada (ayunas o post-comida).</summary>
    public int MinOccurrencesForPatternAlert { get; set; } = 3;

    public string LabelFasting { get; set; } = "En ayunas";
    public string LabelPostMeal { get; set; } = "Después de comer";
}
