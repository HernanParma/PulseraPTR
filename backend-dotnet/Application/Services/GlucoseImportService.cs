using Application.Abstractions;
using Application.Dtos.Glucose;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public sealed class GlucoseImportService : IGlucoseImportService
{
    private readonly IPacienteRepository _pacientes;
    private readonly IGlucoseReadingRepository _readings;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GlucoseImportService> _logger;
    private readonly IPulseraRealtimeNotifier _realtime;
    private readonly MySugrCsvParser _parser = new();

    public GlucoseImportService(
        IPacienteRepository pacientes,
        IGlucoseReadingRepository readings,
        IUnitOfWork uow,
        ILogger<GlucoseImportService> logger,
        IPulseraRealtimeNotifier realtime)
    {
        _pacientes = pacientes;
        _readings = readings;
        _uow = uow;
        _logger = logger;
        _realtime = realtime;
    }

    public async Task<GlucoseImportResultDto> ImportMySugrCsvAsync(
        int pacienteId,
        Stream csvStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (csvStream.CanSeek)
            csvStream.Position = 0;

        _ = await _pacientes.GetByIdAsync(pacienteId, cancellationToken)
            ?? throw new NotFoundException($"No existe el paciente {pacienteId}.");

        var parse = _parser.Parse(csvStream);
        var errors = new List<string>();
        errors.AddRange(parse.HeaderErrors);

        var rowsRead = parse.Rows.Count;
        var imported = 0;
        var duplicates = 0;
        var discarded = 0;

        if (parse.HeaderErrors.Count > 0)
        {
            return new GlucoseImportResultDto
            {
                RowsRead = rowsRead,
                Imported = 0,
                Duplicates = 0,
                Discarded = discarded,
                Errors = errors
            };
        }

        var candidates = new List<GlucoseReading>();
        foreach (var row in parse.Rows)
        {
            var glucoseRaw = row.GlucoseRaw.Trim();
            if (string.IsNullOrEmpty(glucoseRaw))
            {
                discarded++;
                continue;
            }

            if (!int.TryParse(glucoseRaw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var glucose))
            {
                discarded++;
                errors.Add($"Línea {row.LineNumber}: glucemia no numérica '{row.GlucoseRaw}'.");
                continue;
            }

            if (!MySugrSpanishDateTimeParser.TryParseToUtc(row.DateRaw, row.TimeRaw, row.TimeZoneRaw, out var utc, out var dtErr))
            {
                discarded++;
                errors.Add($"Línea {row.LineNumber}: {dtErr}");
                continue;
            }

            var label = row.Label.Trim();
            var hash = GlucoseImportHash.Compute(pacienteId, utc, glucose, label);

            candidates.Add(new GlucoseReading
            {
                PacienteId = pacienteId,
                ReadingDateTime = utc,
                DateRaw = row.DateRaw.Trim(),
                TimeRaw = row.TimeRaw.Trim(),
                Label = label,
                GlucoseMgDl = glucose,
                TimeZone = string.IsNullOrWhiteSpace(row.TimeZoneRaw) ? "GMT-03:00" : row.TimeZoneRaw.Trim(),
                SourceFileName = string.IsNullOrWhiteSpace(fileName) ? null : Path.GetFileName(fileName),
                Source = GlucoseReadingSource.MySugrCsvImport,
                ImportHash = hash,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (candidates.Count == 0)
        {
            return new GlucoseImportResultDto
            {
                RowsRead = rowsRead,
                Imported = 0,
                Duplicates = duplicates,
                Discarded = discarded,
                Errors = errors
            };
        }

        var hashes = candidates.Select(c => c.ImportHash).ToList();
        var existing = await _readings.GetExistingHashesAsync(pacienteId, hashes, cancellationToken);

        var toInsert = new List<GlucoseReading>();
        var pendingHashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in candidates)
        {
            if (existing.Contains(c.ImportHash))
            {
                duplicates++;
                continue;
            }

            if (!pendingHashes.Add(c.ImportHash))
            {
                duplicates++;
                continue;
            }

            toInsert.Add(c);
        }

        if (toInsert.Count > 0)
        {
            await _readings.AddRangeAsync(toInsert, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            imported = toInsert.Count;
            _logger.LogInformation("Importación mySugr: paciente {PacienteId}, nuevas lecturas {Count}.", pacienteId, imported);

            try
            {
                await _realtime.NotificarGlucemiaActualizadaAsync(pacienteId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo notificar actualización de glucemia por SignalR (paciente {PacienteId}).", pacienteId);
            }
        }

        return new GlucoseImportResultDto
        {
            RowsRead = rowsRead,
            Imported = imported,
            Duplicates = duplicates,
            Discarded = discarded,
            Errors = errors
        };
    }
}
