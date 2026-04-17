using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IAlertaRepository
{
    Task AddAsync(Alerta alerta, CancellationToken cancellationToken = default);
    Task<Alerta?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alerta>> BuscarAsync(int? pacienteId, bool? leida, CancellationToken cancellationToken = default);
    void Update(Alerta alerta);
    Task<int> ContarActivasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alerta>> GetUltimasAsync(int cantidad, CancellationToken cancellationToken = default);
}
