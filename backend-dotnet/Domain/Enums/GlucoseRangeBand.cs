namespace Domain.Enums;

/// <summary>
/// Banda visual/clínica para UI y reglas (umbrales configurables en appsettings).
/// </summary>
public enum GlucoseRangeBand
{
    SevereHypoglycemia = 1,
    Hypoglycemia = 2,
    Normal = 3,
    Hyperglycemia = 4,
    SevereHyperglycemia = 5,
    CriticalIsolated = 6
}
