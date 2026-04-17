using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IEventoEmergenciaRepository
{
    Task AddAsync(EventoEmergencia evento, CancellationToken cancellationToken = default);
    Task<EventoEmergencia?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventoEmergencia>> BuscarAsync(
        int? pacienteId,
        bool? atendido,
        TipoEventoEmergencia? tipoEvento,
        CancellationToken cancellationToken = default);

    void Update(EventoEmergencia evento);
    Task<int> ContarSosPendientesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventoEmergencia>> GetUltimosAsync(int cantidad, TipoEventoEmergencia tipoEvento, CancellationToken cancellationToken = default);
}
