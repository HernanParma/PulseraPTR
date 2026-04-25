using System.Globalization;
using System.Text;

namespace Application.Services;

public sealed class MySugrCsvParser
{
    public const string HeaderFecha = "Fecha";
    public const string HeaderHora = "Hora";
    public const string HeaderEtiquetas = "Etiquetas";
    public const string HeaderGlucosa = "Medición del azúcar en sangre (mg/dL)";
    public const string HeaderZona = "Zona horaria";

    public sealed class Row
    {
        public int LineNumber { get; init; }
        public string DateRaw { get; init; } = string.Empty;
        public string TimeRaw { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string GlucoseRaw { get; init; } = string.Empty;
        public string TimeZoneRaw { get; init; } = string.Empty;
    }

    public sealed class Result
    {
        public char Delimiter { get; init; }
        public IReadOnlyDictionary<string, int> HeaderIndex { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
        public IReadOnlyList<Row> Rows { get; init; } = Array.Empty<Row>();
        public IReadOnlyList<string> HeaderErrors { get; init; } = Array.Empty<string>();
    }

    public Result Parse(Stream stream, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        var firstLine = reader.ReadLine();
        if (firstLine is null)
        {
            return new Result
            {
                Delimiter = ',',
                HeaderErrors = new[] { "Archivo vacío." }
            };
        }

        var delimiter = DetectDelimiter(firstLine);
        var headerFields = SplitCsvLine(firstLine, delimiter);
        var headerIndex = BuildHeaderIndex(headerFields, out var headerErrors);

        var rows = new List<Row>();
        var lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = SplitCsvLine(line, delimiter);
            rows.Add(MapRow(lineNumber, headerIndex, fields, headerErrors.Count == 0));
        }

        return new Result
        {
            Delimiter = delimiter,
            HeaderIndex = headerIndex,
            Rows = rows,
            HeaderErrors = headerErrors
        };
    }

    private static Row MapRow(int lineNumber, IReadOnlyDictionary<string, int> headerIndex, IReadOnlyList<string> fields, bool headerOk)
    {
        if (!headerOk)
            return new Row { LineNumber = lineNumber };

        string Get(string key) =>
            TryGet(headerIndex, fields, key, out var v) ? NormalizeCell(v) : string.Empty;

        return new Row
        {
            LineNumber = lineNumber,
            DateRaw = Get(HeaderFecha),
            TimeRaw = Get(HeaderHora),
            Label = Get(HeaderEtiquetas),
            GlucoseRaw = Get(HeaderGlucosa),
            TimeZoneRaw = Get(HeaderZona)
        };
    }

    private static bool TryGet(IReadOnlyDictionary<string, int> headerIndex, IReadOnlyList<string> fields, string key, out string value)
    {
        value = string.Empty;
        if (!headerIndex.TryGetValue(key, out var idx) || idx < 0 || idx >= fields.Count)
            return false;

        value = fields[idx].Trim();
        return true;
    }

    private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headerFields, out List<string> errors)
    {
        errors = new List<string>();
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < headerFields.Count; i++)
        {
            var name = NormalizeHeaderName(headerFields[i]);
            if (string.IsNullOrEmpty(name))
                continue;

            map[name] = i;
        }

        foreach (var required in new[] { HeaderFecha, HeaderHora, HeaderGlucosa })
        {
            if (!map.ContainsKey(required))
                errors.Add($"Falta columna requerida: '{required}'.");
        }

        return map;
    }

    private static string NormalizeCell(string raw) =>
        raw.Trim().TrimStart('\uFEFF');

    private static string NormalizeHeaderName(string raw)
    {
        var s = raw.Trim().Trim('"').Trim().TrimStart('\uFEFF');
        return string.IsNullOrEmpty(s) ? string.Empty : s;
    }

    private static char DetectDelimiter(string headerLine)
    {
        var commas = headerLine.Count(c => c == ',');
        var semis = headerLine.Count(c => c == ';');
        return semis > commas ? ';' : ',';
    }

    /// <summary>
    /// Parser CSV mínimo con soporte de comillas dobles (RFC4180 básico, una línea = un registro).
    /// </summary>
    public static IReadOnlyList<string> SplitCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    var isEscapedQuote = i + 1 < line.Length && line[i + 1] == '"';
                    if (isEscapedQuote)
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }

                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
                continue;
            }

            if (c == delimiter)
            {
                fields.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(c);
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
