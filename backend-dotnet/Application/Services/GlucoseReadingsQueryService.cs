using Application.Configuration;
using Application.Dtos.Glucose;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Exceptions;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services;

public sealed class GlucoseReadingsQueryService : IGlucoseReadingsQueryService
{
    private readonly IPacienteRepository _pacientes;
    private readonly IGlucoseReadingRepository _readings;
    private readonly IGlucoseAlertEvaluator _alerts;
    private readonly GlucoseAlertOptions _alertOpt;
    private readonly GlucoseDashboardOptions _dashOpt;

    public GlucoseReadingsQueryService(
        IPacienteRepository pacientes,
        IGlucoseReadingRepository readings,
        IGlucoseAlertEvaluator alerts,
        IOptions<GlucoseAlertOptions> alertOptions,
        IOptions<GlucoseDashboardOptions> dashOptions)
    {
        _pacientes = pacientes;
        _readings = readings;
        _alerts = alerts;
        _alertOpt = alertOptions.Value;
        _dashOpt = dashOptions.Value;
    }

    public async Task<GlucoseDashboardDto> GetDashboardAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        await EnsurePacienteExists(pacienteId, cancellationToken);

        var to = DateTime.UtcNow;
        var from = to.AddDays(-_dashOpt.SummaryWindowDays);

        var window = await _readings.GetForPatientAsync(pacienteId, from, to, cancellationToken);
        var latest = await _readings.GetLatestAsync(pacienteId, cancellationToken);

        var hypo = window.Count(r => r.GlucoseMgDl < _alertOpt.HypoglycemiaMaxExclusive);
        var hyper = window.Count(r => r.GlucoseMgDl > _alertOpt.HyperglycemiaMinExclusive);

        int? avg = null;
        int? min = null;
        int? max = null;
        if (window.Count > 0)
        {
            avg = (int)Math.Round(window.Average(r => r.GlucoseMgDl));
            min = window.Min(r => r.GlucoseMgDl);
            max = window.Max(r => r.GlucoseMgDl);
        }

        var recentEntities = window
            .OrderByDescending(r => r.ReadingDateTime)
            .Take(_dashOpt.RecentReadingsTake)
            .ToList();

        var recent = recentEntities
            .Select(r => GlucoseReadingMapper.ToDto(r, _alertOpt))
            .ToList();

        var orderedForChart = window.OrderBy(r => r.ReadingDateTime).ToList();
        var chart = SampleChart(orderedForChart, _dashOpt.ChartMaxPoints);

        var alertItems = _alerts.BuildAlerts(window);
        var highlighted = alertItems.Count;

        return new GlucoseDashboardDto
        {
            PacienteId = pacienteId,
            LastReading = latest is null ? null : GlucoseReadingMapper.ToDto(latest, _alertOpt),
            AverageMgDl = avg,
            MinMgDl = min,
            MaxMgDl = max,
            HypoglycemiaCount = hypo,
            HyperglycemiaCount = hyper,
            HighlightedAlertCount = highlighted,
            RecentReadings = recent,
            Alerts = alertItems,
            ChartPoints = chart,
            SummaryFromUtc = from,
            SummaryToUtc = to
        };
    }

    public async Task<IReadOnlyList<GlucoseReadingDto>> GetReadingsAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        await EnsurePacienteExists(pacienteId, cancellationToken);

        var recent = await _readings.GetRecentAsync(pacienteId, _dashOpt.RecentReadingsTake, cancellationToken);
        return recent.Select(r => GlucoseReadingMapper.ToDto(r, _alertOpt)).ToList();
    }

    public async Task<IReadOnlyList<GlucoseAlertItemDto>> GetAlertsAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        await EnsurePacienteExists(pacienteId, cancellationToken);

        var to = DateTime.UtcNow;
        var from = to.AddDays(-_dashOpt.SummaryWindowDays);
        var window = await _readings.GetForPatientAsync(pacienteId, from, to, cancellationToken);
        return _alerts.BuildAlerts(window);
    }

    private async Task EnsurePacienteExists(int pacienteId, CancellationToken cancellationToken)
    {
        var p = await _pacientes.GetByIdAsync(pacienteId, cancellationToken);
        if (p is null)
            throw new NotFoundException($"No existe el paciente {pacienteId}.");
    }

    private static IReadOnlyList<GlucoseChartPointDto> SampleChart(IReadOnlyList<GlucoseReading> ordered, int maxPoints)
    {
        if (ordered.Count == 0)
            return Array.Empty<GlucoseChartPointDto>();

        if (ordered.Count <= maxPoints)
        {
            return ordered
                .Select(r => new GlucoseChartPointDto
                {
                    AtUtc = r.ReadingDateTime,
                    GlucoseMgDl = r.GlucoseMgDl,
                    Label = r.Label
                })
                .ToList();
        }

        var step = (double)(ordered.Count - 1) / (maxPoints - 1);
        var points = new List<GlucoseChartPointDto>(maxPoints);
        for (var i = 0; i < maxPoints; i++)
        {
            var idx = (int)Math.Round(i * step);
            idx = Math.Clamp(idx, 0, ordered.Count - 1);
            var r = ordered[idx];
            points.Add(new GlucoseChartPointDto
            {
                AtUtc = r.ReadingDateTime,
                GlucoseMgDl = r.GlucoseMgDl,
                Label = r.Label
            });
        }

        return points;
    }
}
