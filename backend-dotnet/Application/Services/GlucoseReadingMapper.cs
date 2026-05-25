using Application.Configuration;
using Application.Dtos.Glucose;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public static class GlucoseReadingMapper
{
    public static GlucoseReadingDto ToDto(GlucoseReading r, GlucoseAlertOptions thresholds)
    {
        var band = ClassifyBand(r.GlucoseMgDl, thresholds);
        return new GlucoseReadingDto
        {
            Id = r.Id,
            PacienteId = r.PacienteId,
            ReadingDateTimeUtc = r.ReadingDateTime,
            DateRaw = r.DateRaw,
            TimeRaw = r.TimeRaw,
            Label = r.Label,
            GlucoseMgDl = r.GlucoseMgDl,
            TimeZone = r.TimeZone,
            SourceFileName = r.SourceFileName,
            Source = r.Source,
            Band = band
        };
    }

    public static GlucoseRangeBand ClassifyBand(int glucoseMgDl, GlucoseAlertOptions o)
    {
        if (glucoseMgDl > o.CriticalIsolatedMinExclusive)
            return GlucoseRangeBand.CriticalIsolated;

        if (glucoseMgDl < o.SevereHypoglycemiaMaxExclusive)
            return GlucoseRangeBand.SevereHypoglycemia;

        if (glucoseMgDl < o.HypoglycemiaMaxExclusive)
            return GlucoseRangeBand.Hypoglycemia;

        if (glucoseMgDl > o.SevereHyperglycemiaMinExclusive)
            return GlucoseRangeBand.SevereHyperglycemia;

        if (glucoseMgDl > o.HyperglycemiaMinExclusive)
            return GlucoseRangeBand.Hyperglycemia;

        return GlucoseRangeBand.Normal;
    }
}
