using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Textos de alerta cuando la APK no envía <c>mensajeAlerta</c>.
/// </summary>
internal static class MedicionMensajeBuilder
{
    public static string DesdeMedicion(Medicion m) =>
        m.Tipo switch
        {
            TipoMedicion.FrecuenciaCardiaca => MensajeFrecuenciaCardiaca(m.ValorMedicion, m.Estado),
            _ => m.Estado switch
            {
                EstadoClinico.CRITICO => "Valor fuera de rango crítico",
                EstadoClinico.ADVERTENCIA => "Valor fuera de rango",
                _ => "Medición dentro del rango normal"
            }
        };

    private static string MensajeFrecuenciaCardiaca(int lpm, EstadoClinico estado) =>
        estado switch
        {
            EstadoClinico.CRITICO when lpm >= 120 => "Frecuencia cardíaca críticamente alta",
            EstadoClinico.CRITICO when lpm <= 50 => "Frecuencia cardíaca críticamente baja",
            EstadoClinico.ADVERTENCIA when lpm > 100 => "Frecuencia cardíaca alta",
            EstadoClinico.ADVERTENCIA when lpm < 60 => "Frecuencia cardíaca baja",
            EstadoClinico.NORMAL => "Frecuencia cardíaca normal",
            _ => $"Frecuencia cardíaca: {lpm} lpm"
        };
}
