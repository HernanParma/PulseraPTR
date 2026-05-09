using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    internal class RangoValoresMedicionService : IRangoValoresMedicionService
    {
        private readonly IRangoValoresMedicionRepository _rangosValoresMedicion;

        public RangoValoresMedicionService(IRangoValoresMedicionRepository rangosValoresMedicion)
        {
            _rangosValoresMedicion = rangosValoresMedicion;
        }

        public async Task<List<RangoValoresMedicionDto>> ConsultarRangos()
        {
            var rangos = await _rangosValoresMedicion.GetAll();

            return rangos.Select(MapToDto).ToList();
        }

        public async Task<RangoValoresMedicionDto> CrearRango(CrearRangoValoresMedicionDto dto)
        {
            var tipoMedicion = (TipoMedicion)Enum.Parse(typeof(TipoMedicion), dto.TipoMedicion);

            var nuevoRango = new RangoValoresMedicion(tipoMedicion,
                                                       dto.EdadMinima,
                                                       dto.EdadMaxima,
                                                       dto.ValorNormalMinimo,
                                                       dto.ValorNormalMaximo,
                                                       dto.ValorCriticoMinimo,
                                                       dto.ValorNormalMaximo);

            nuevoRango = await _rangosValoresMedicion.Insert(nuevoRango);

            return MapToDto(nuevoRango);
        }

        private static RangoValoresMedicionDto MapToDto(RangoValoresMedicion rango) =>
            new(rango.Tipo.ToString(),
                rango.RangoEdadMinimo,
                rango.RangoEdadMaximo,
                rango.ValorNormalMinimo,
                rango.ValorNormalMaximo,
                rango.ValorCriticoMinimo,
                rango.ValorCriticoMaximo);


    }
}
