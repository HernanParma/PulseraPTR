using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IPacienteRepository
{
    Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Paciente>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken = default);
    Task<int> ContarActivosAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default);
    void Update(Paciente paciente);
}
