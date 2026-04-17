using Application.Dtos;

namespace Application.Interfaces;

public interface IAlertaService
{
    Task<IReadOnlyList<AlertaDto>> ListarAsync(int? pacienteId, bool? leida, CancellationToken cancellationToken = default);
    Task MarcarLeidaAsync(int id, CancellationToken cancellationToken = default);
}
