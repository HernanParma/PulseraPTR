using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IGlucoseReadingRepository
{
    Task<IReadOnlySet<string>> GetExistingHashesAsync(int pacienteId, IReadOnlyCollection<string> hashes, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<GlucoseReading> readings, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseReading>> GetForPatientAsync(int pacienteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<GlucoseReading?> GetLatestAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseReading>> GetRecentAsync(int pacienteId, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlucoseReading>> GetRecentAcrossPatientsAsync(int take, CancellationToken cancellationToken = default);
}
