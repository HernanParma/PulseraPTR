using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Services;

/// <summary>
/// Combina fecha tipo mySugr en español ("17 abr 2026") con hora tipo "6:52:22 p. m." y un offset tipo "GMT-03:00".
/// La lógica es delicada: mySugr localiza meses y meridianos en español; normalizamos a invariante antes de parsear.
/// </summary>
public static class MySugrSpanishDateTimeParser
{
    private static readonly Dictionary<string, int> SpanishMonthAbbrev = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ene"] = 1, ["feb"] = 2, ["mar"] = 3, ["abr"] = 4, ["may"] = 5, ["jun"] = 6,
        ["jul"] = 7, ["ago"] = 8, ["sep"] = 9, ["sept"] = 9,
        ["oct"] = 10, ["nov"] = 11, ["dic"] = 12
    };

    /// <summary>
    /// Intenta producir UTC. Si la zona viene vacía, por defecto GMT-03:00 (uso frecuente en exportes AR).
    /// </summary>
    public static bool TryParseToUtc(string dateRaw, string timeRaw, string timeZoneRaw, out DateTime utc, out string? error)
    {
        utc = default;
        error = null;

        var d = dateRaw.Trim();
        var t = timeRaw.Trim();
        if (string.IsNullOrEmpty(d) || string.IsNullOrEmpty(t))
        {
            error = "Fecha u hora vacía.";
            return false;
        }

        if (!TryParseSpanishDate(d, out var year, out var month, out var day))
        {
            error = $"Fecha no reconocida: '{dateRaw}'.";
            return false;
        }

        if (!TryParseSpanishTimeWithMeridiem(t, out var hour, out var minute, out var second))
        {
            error = $"Hora no reconocida: '{timeRaw}'.";
            return false;
        }

        if (!TryParseGmtOffset(string.IsNullOrWhiteSpace(timeZoneRaw) ? "GMT-03:00" : timeZoneRaw.Trim(), out var offset))
        {
            error = $"Zona horaria no reconocida: '{timeZoneRaw}'.";
            return false;
        }

        var local = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
        try
        {
            var dto = new DateTimeOffset(local, offset);
            utc = dto.UtcDateTime;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Combinación fecha/hora inválida: {ex.Message}";
            return false;
        }
    }

    private static bool TryParseSpanishDate(string dateRaw, out int year, out int month, out int day)
    {
        year = month = day = default;
        var parts = dateRaw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return false;

        if (!int.TryParse(parts[0].Trim().TrimStart('\uFEFF'), NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
            return false;

        var monthToken = parts[1].Trim().TrimStart('\uFEFF').TrimEnd('.');
        if (!SpanishMonthAbbrev.TryGetValue(monthToken, out month))
            return false;

        if (!int.TryParse(parts[2].Trim().TrimStart('\uFEFF'), NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
            return false;

        return day is >= 1 and <= 31;
    }

    /// <summary>
    /// Convierte "6:52:22 p. m." / "11:22:58 a. m." a componentes 24h.
    /// </summary>
    private static bool TryParseSpanishTimeWithMeridiem(string timeRaw, out int hour, out int minute, out int second)
    {
        hour = minute = second = default;
        var normalized = NormalizeSpanishMeridiem(timeRaw);

        // Tras normalizar meridianos, probamos formatos comunes en invariante.
        var formats = new[]
        {
            "h:mm:ss tt",
            "hh:mm:ss tt",
            "h:mm tt",
            "hh:mm tt",
            "H:mm:ss",
            "HH:mm:ss"
        };

        if (DateTime.TryParseExact(normalized, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            hour = dt.Hour;
            minute = dt.Minute;
            second = dt.Second;
            return true;
        }

        // Fallback: parse libre por si el export trae variantes raras.
        if (DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            hour = dt.Hour;
            minute = dt.Minute;
            second = dt.Second;
            return true;
        }

        return false;
    }

    private static string NormalizeSpanishMeridiem(string timeRaw)
    {
        var s = timeRaw.Trim().TrimStart('\uFEFF');
        // mySugr: "6:52:22 p. m." — también tolerar "p.m." pegado a los segundos (sin \b entre dígito y "p").
        s = Regex.Replace(s, @"p\.\s*m\.", "PM", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"a\.\s*m\.", "AM", RegexOptions.IgnoreCase);
        // Caso sin puntos: "p m" -> PM
        s = Regex.Replace(s, @"\bp\s+m\b", "PM", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\ba\s+m\b", "AM", RegexOptions.IgnoreCase);
        return s.Trim();
    }

    private static bool TryParseGmtOffset(string tz, out TimeSpan offset)
    {
        offset = default;
        var m = Regex.Match(tz.Trim(), @"^GMT(?<sign>[+-])(?<hh>\d{1,2}):(?<mm>\d{2})$", RegexOptions.IgnoreCase);
        if (!m.Success)
            return false;

        var sign = m.Groups["sign"].Value == "-" ? -1 : 1;
        if (!int.TryParse(m.Groups["hh"].Value, out var hh))
            return false;
        if (!int.TryParse(m.Groups["mm"].Value, out var mm))
            return false;

        offset = new TimeSpan(sign * hh, mm, 0);
        return true;
    }
}
