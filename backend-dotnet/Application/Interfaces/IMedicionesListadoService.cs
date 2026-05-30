using Application.Dtos;
using Domain.Enums;

namespace Application.Interfaces;

public interface IMedicionesListadoService
{
    Task<MedicionesIndexDto> ObtenerIndexAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        int pagina = 1,
        int tamanoPagina = MedicionesIndexDto.TamanoPaginaPorDefecto,
        CancellationToken cancellationToken = default);
}

public sealed class MedicionesIndexDto
{
    public const int TamanoPaginaPorDefecto = 20;

    public IReadOnlyList<MedicionDto> MedicionesPulsera { get; init; } = Array.Empty<MedicionDto>();
    public IReadOnlyList<MedicionListadoFilaDto> Filas { get; init; } = Array.Empty<MedicionListadoFilaDto>();
    public int TotalFilas { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanoPagina { get; init; } = TamanoPaginaPorDefecto;
    public int TotalPaginas { get; init; } = 1;

    public int IndicePrimerRegistro => TotalFilas == 0 ? 0 : (Pagina - 1) * TamanoPagina + 1;
    public int IndiceUltimoRegistro => TotalFilas == 0 ? 0 : Math.Min(Pagina * TamanoPagina, TotalFilas);
}
