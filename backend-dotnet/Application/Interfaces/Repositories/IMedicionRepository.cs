using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IMedicionRepository
{
    Task AddAsync(Medicion medicion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Medicion>> BuscarAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        bool? soloFueraDeRango,
        CancellationToken cancellationToken = default);

    Task<Medicion?> GetUltimaPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task<Medicion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    void Remove(Medicion medicion);

    Task<int> ContarFueraDeRangoAsync(CancellationToken cancellationToken = default);
}
