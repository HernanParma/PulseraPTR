using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

public class ClasificacionEstadoCardiaco : IClasificacionEstadoCardiaco
{
    public EstadoClinico CalcularPorFrecuenciaCardiaca(int frecuenciaCardiaca)
    {
        if (frecuenciaCardiaca >= 120)
            return EstadoClinico.CRITICO;

        if (frecuenciaCardiaca > 100)
            return EstadoClinico.ADVERTENCIA;

        if (frecuenciaCardiaca < 50)
            return EstadoClinico.ADVERTENCIA;

        return EstadoClinico.NORMAL;
    }

    public bool EsFueraDeRango(EstadoClinico estado) =>
        estado is EstadoClinico.ADVERTENCIA or EstadoClinico.CRITICO;

    public string ObtenerMensajeSugerido(EstadoClinico estado, int frecuenciaCardiaca) =>
        estado switch
        {
            EstadoClinico.NORMAL => $"Frecuencia cardíaca normal ({frecuenciaCardiaca} lpm).",
            EstadoClinico.ADVERTENCIA => $"Frecuencia cardíaca en advertencia ({frecuenciaCardiaca} lpm).",
            EstadoClinico.CRITICO => $"Frecuencia cardíaca crítica ({frecuenciaCardiaca} lpm).",
            _ => $"Medición registrada ({frecuenciaCardiaca} lpm)."
        };
}
