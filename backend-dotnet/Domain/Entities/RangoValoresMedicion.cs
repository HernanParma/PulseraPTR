using Domain.Enums;

namespace Domain.Entities
{
    public class RangoValoresMedicion
    {

        public string Id { get; set; }
        public TipoMedicion TipoMedicion { get; set; }
        public int RangoEdadMinimo { get; set; }
        public int RangoEdadMaximo { get; set; }
        public int ValorNormalMinimo { get; set; }
        public int ValorNormalMaximo { get; set; }
        public int ValorCriticoMinimo { get; set; }
        public int ValorCriticoMaximo { get; set; }

        public RangoValoresMedicion(TipoMedicion tipo,
                            int rangoEdadMinimo,
                            int rangoEdadMaximo,
                            int valorNormalMinimo,
                            int valorNormalMaximo,
                            int valorCriticoMinimo,
                            int valorCriticoMaximo)
        {
            Id = Guid.NewGuid().ToString();
            TipoMedicion = tipo;
            RangoEdadMinimo = rangoEdadMinimo;
            RangoEdadMaximo = rangoEdadMaximo;
            ValorNormalMinimo = valorNormalMinimo;
            ValorNormalMaximo = valorNormalMaximo;
            ValorCriticoMinimo = valorCriticoMinimo;
            ValorCriticoMaximo = valorCriticoMaximo;
        }

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
