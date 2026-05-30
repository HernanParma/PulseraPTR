using Application.Dtos;

namespace Application.Interfaces;

public interface IAlertaService
{
    Task<IReadOnlyList<AlertaDto>> ListarAsync(int? pacienteId, bool? leida, CancellationToken cancellationToken = default);
    Task<int> ContarSinLeerAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertaDto>> ListarRecientesAsync(int cantidad = 12, CancellationToken cancellationToken = default);
    Task MarcarLeidaAsync(int id, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
