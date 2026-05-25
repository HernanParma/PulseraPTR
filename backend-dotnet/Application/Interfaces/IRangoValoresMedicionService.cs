using Application.Dtos;

namespace Application.Interfaces
{
    public interface IRangoValoresMedicionService
    {
        Task<RangoValoresMedicionDto> CrearRango(CrearRangoValoresMedicionDto nuevoRango);
        Task<List<RangoValoresMedicionDto>> ConsultarRangos();
    }
}
