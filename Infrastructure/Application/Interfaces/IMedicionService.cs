using Application.Dtos;
using Domain.Enums;

namespace Application.Interfaces;

public interface IMedicionService
{
    Task<MedicionDto> RegistrarAsync(CrearMedicionDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicionDto>> ListarAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicionDto>> ListarFueraDeRangoAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicionDto>> ListarPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
