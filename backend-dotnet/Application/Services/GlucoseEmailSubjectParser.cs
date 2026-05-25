using System.Text.RegularExpressions;

namespace Application.Services;

/// <summary>
/// Extrae el paciente desde el asunto (v1). Ej: <c>mySugr PacienteId: 22</c>.
/// </summary>
public static partial class GlucoseEmailSubjectParser
{
    [GeneratedRegex(@"PacienteId\s*:\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PacienteIdRegex();

    public static bool TryExtractPacienteId(string? subject, out int pacienteId)
    {
        pacienteId = default;
        if (string.IsNullOrWhiteSpace(subject))
            return false;

        var m = PacienteIdRegex().Match(subject.Trim());
        if (!m.Success)
            return false;

        return int.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out pacienteId)
            && pacienteId > 0;
    }
}
