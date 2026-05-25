using Application.Configuration;
using Application.Dtos.Glucose;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services;

/// <summary>
/// Centraliza reglas clínicas y de patrón (ventanas móviles). Los umbrales salen de configuración.
/// </summary>
public sealed class GlucoseAlertEvaluator : IGlucoseAlertEvaluator
{
    private readonly GlucoseAlertOptions _opt;

    public GlucoseAlertEvaluator(IOptions<GlucoseAlertOptions> options) =>
        _opt = options.Value;

    public IReadOnlyList<GlucoseAlertItemDto> BuildAlerts(IReadOnlyList<GlucoseReading> readingsInSummaryWindow)
    {
        var list = new List<GlucoseAlertItemDto>();
        if (readingsInSummaryWindow.Count == 0)
            return list;

        var now = DateTime.UtcNow;
        var rollFrom = now.AddDays(-_opt.RollingDays);

        foreach (var r in readingsInSummaryWindow.OrderByDescending(x => x.ReadingDateTime))
        {
            foreach (var a in EvaluateThresholdRules(r))
                list.Add(a);
        }

        var rolling = readingsInSummaryWindow
            .Where(r => r.ReadingDateTime >= rollFrom)
            .ToList();

        var fastingHigh = rolling.Count(r =>
            string.Equals(r.Label.Trim(), _opt.LabelFasting, StringComparison.OrdinalIgnoreCase) &&
            r.GlucoseMgDl > _opt.FastingHighThresholdExclusive);

        if (fastingHigh >= _opt.MinOccurrencesForPatternAlert)
        {
            list.Add(new GlucoseAlertItemDto
            {
                Code = "FASTING_HIGH_PATTERN",
                Severity = "Warning",
                Message = $"En los últimos {_opt.RollingDays} días hay {fastingHigh} lecturas en ayunas por encima de {_opt.FastingHighThresholdExclusive} mg/dL.",
                OccurredAtUtc = now
            });
        }

        var postMealHigh = rolling.Count(r =>
            string.Equals(r.Label.Trim(), _opt.LabelPostMeal, StringComparison.OrdinalIgnoreCase) &&
            r.GlucoseMgDl > _opt.PostMealHighThresholdExclusive);

        if (postMealHigh >= _opt.MinOccurrencesForPatternAlert)
        {
            list.Add(new GlucoseAlertItemDto
            {
                Code = "POSTMEAL_HIGH_PATTERN",
                Severity = "Warning",
                Message = $"En los últimos {_opt.RollingDays} días hay {postMealHigh} lecturas después de comer por encima de {_opt.PostMealHighThresholdExclusive} mg/dL.",
                OccurredAtUtc = now
            });
        }

        return list
            .OrderByDescending(a => SeverityRank(a.Severity))
            .ThenByDescending(a => a.OccurredAtUtc)
            .ToList();
    }

    private IEnumerable<GlucoseAlertItemDto> EvaluateThresholdRules(GlucoseReading r)
    {
        var g = r.GlucoseMgDl;

        if (g > _opt.CriticalIsolatedMinExclusive)
        {
            yield return new GlucoseAlertItemDto
            {
                Code = "CRITICAL_HIGH",
                Severity = "Critical",
                Message = $"Valor crítico aislado: {g} mg/dL.",
                OccurredAtUtc = r.ReadingDateTime,
                RelatedReadingId = r.Id
            };
            yield break;
        }

        if (g < _opt.SevereHypoglycemiaMaxExclusive)
        {
            yield return new GlucoseAlertItemDto
            {
                Code = "SEVERE_HYPO",
                Severity = "Critical",
                Message = $"Hipoglucemia severa: {g} mg/dL.",
                OccurredAtUtc = r.ReadingDateTime,
                RelatedReadingId = r.Id
            };
            yield break;
        }

        if (g < _opt.HypoglycemiaMaxExclusive)
        {
            yield return new GlucoseAlertItemDto
            {
                Code = "HYPO",
                Severity = "Warning",
                Message = $"Hipoglucemia: {g} mg/dL.",
                OccurredAtUtc = r.ReadingDateTime,
                RelatedReadingId = r.Id
            };
            yield break;
        }

        if (g > _opt.SevereHyperglycemiaMinExclusive)
        {
            yield return new GlucoseAlertItemDto
            {
                Code = "SEVERE_HYPER",
                Severity = "Warning",
                Message = $"Hiperglucemia severa: {g} mg/dL.",
                OccurredAtUtc = r.ReadingDateTime,
                RelatedReadingId = r.Id
            };
            yield break;
        }

        if (g > _opt.HyperglycemiaMinExclusive)
        {
            yield return new GlucoseAlertItemDto
            {
                Code = "HYPER",
                Severity = "Info",
                Message = $"Hiperglucemia: {g} mg/dL.",
                OccurredAtUtc = r.ReadingDateTime,
                RelatedReadingId = r.Id
            };
        }
    }

    private static int SeverityRank(string severity) =>
        severity switch
        {
            "Critical" => 3,
            "Warning" => 2,
            _ => 1
        };
}
