namespace Domain.Enums;

/// <summary>
/// Origen del registro (importación manual, CSV mySugr, futuras integraciones API).
/// </summary>
public enum GlucoseReadingSource
{
    MySugrCsvImport = 1,
    ManualEntry = 2,
    ApiIntegration = 3
}
