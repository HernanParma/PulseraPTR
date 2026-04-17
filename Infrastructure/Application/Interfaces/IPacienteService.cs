using Application.Dtos;

namespace Application.Interfaces;

public interface IPacienteService
{
    Task<IReadOnlyList<PacienteDto>> ListarAsync(bool incluirInactivos, CancellationToken cancellationToken = default);
    Task<PacienteDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PacienteDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);
    Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken cancellationToken = default);
    Task DarDeBajaLogicaAsync(int id, CancellationToken cancellationToken = default);
}
