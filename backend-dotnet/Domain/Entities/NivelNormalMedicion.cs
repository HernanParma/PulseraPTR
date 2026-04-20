using Domain.Enums;

namespace Domain.Entities
{
    public class NivelNormalMedicion
    {
        public TipoMedicion Tipo { get; set; }
        public int RangoEdadMinimo { get; set; }
        public int RangoEdadMaximo { get; set; }
        public int ValorNormalMinimo { get; set; }
        public int ValorNormalMaximo { get; set; }
        public int ValorCriticoMinimo { get; set; }
        public int ValorCriticoMaximo { get; set; }

        public EstadoClinico Clasificar(int valorMedicion)
        {
            if (valorMedicion < ValorCriticoMinimo || valorMedicion > ValorCriticoMaximo)
            {
                return EstadoClinico.CRITICO;
            }

            if (valorMedicion < ValorNormalMinimo || valorMedicion > ValorNormalMaximo)
            {
                return EstadoClinico.ADVERTENCIA;
            }

            return EstadoClinico.NORMAL;
        }
    }
}
