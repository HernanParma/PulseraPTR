using Domain.Enums;

namespace Application.Interfaces;

/// <summary>
/// Reglas centralizadas de clasificación por frecuencia cardíaca (capa Application).
/// </summary>
public interface IClasificacionEstadoCardiaco
{
    EstadoClinico CalcularPorFrecuenciaCardiaca(int frecuenciaCardiaca);

    bool EsFueraDeRango(EstadoClinico estado);

    string ObtenerMensajeSugerido(EstadoClinico estado, int frecuenciaCardiaca);
}
