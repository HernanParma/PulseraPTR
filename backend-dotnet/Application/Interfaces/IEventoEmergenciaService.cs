using Application.Dtos;

namespace Application.Interfaces;

public interface IEventoEmergenciaService
{
    Task<EventoEmergenciaDto> RegistrarSosAsync(CrearEventoEmergenciaDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventoEmergenciaDto>> ListarAsync(int? pacienteId, bool? atendido, CancellationToken cancellationToken = default);
    Task MarcarAtendidoAsync(int id, CancellationToken cancellationToken = default);
}
