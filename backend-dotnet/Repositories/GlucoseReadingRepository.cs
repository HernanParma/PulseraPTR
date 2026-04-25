using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class GlucoseReadingRepository : IGlucoseReadingRepository
{
    private readonly AppDbContext _db;

    public GlucoseReadingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(IEnumerable<GlucoseReading> readings, CancellationToken cancellationToken = default) =>
        await _db.GlucoseReadings.AddRangeAsync(readings, cancellationToken);

    public async Task<IReadOnlySet<string>> GetExistingHashesAsync(
        int pacienteId,
        IReadOnlyCollection<string> hashes,
        CancellationToken cancellationToken = default)
    {
        if (hashes.Count == 0)
            return new HashSet<string>();

        var list = hashes.Distinct().ToList();
        var rows = await _db.GlucoseReadings.AsNoTracking()
            .Where(r => r.PacienteId == pacienteId && list.Contains(r.ImportHash))
            .Select(r => r.ImportHash)
            .ToListAsync(cancellationToken);

        return rows.ToHashSet();
    }

    public async Task<IReadOnlyList<GlucoseReading>> GetForPatientAsync(
        int pacienteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        await _db.GlucoseReadings.AsNoTracking()
            .Where(r => r.PacienteId == pacienteId && r.ReadingDateTime >= fromUtc && r.ReadingDateTime <= toUtc)
            .OrderBy(r => r.ReadingDateTime)
            .ToListAsync(cancellationToken);

    public async Task<GlucoseReading?> GetLatestAsync(int pacienteId, CancellationToken cancellationToken = default) =>
        await _db.GlucoseReadings.AsNoTracking()
            .Where(r => r.PacienteId == pacienteId)
            .OrderByDescending(r => r.ReadingDateTime)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<GlucoseReading>> GetRecentAsync(int pacienteId, int take, CancellationToken cancellationToken = default) =>
        await _db.GlucoseReadings.AsNoTracking()
            .Where(r => r.PacienteId == pacienteId)
            .OrderByDescending(r => r.ReadingDateTime)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GlucoseReading>> GetRecentAcrossPatientsAsync(int take, CancellationToken cancellationToken = default) =>
        await _db.GlucoseReadings.AsNoTracking()
            .Include(r => r.Paciente)
            .OrderByDescending(r => r.ReadingDateTime)
            .Take(take)
            .ToListAsync(cancellationToken);
}
