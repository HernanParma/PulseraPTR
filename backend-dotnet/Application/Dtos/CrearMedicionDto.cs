using System.Text.Json.Serialization;
using Domain.Enums;

namespace Application.Dtos;

public class CrearMedicionDto
{
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int Valor { get; set; }
    public TipoMedicion Tipo { get; set; }
    public int? PasosActividad { get; set; }
    public int? NivelEstres { get; set; }
    public int? MinutosSueno { get; set; }
    public int? MinutosActividad { get; set; }
    public int? CaloriasQuemadas { get; set; }
    public string OrigenDato { get; set; } = string.Empty;

    /// <summary>
    /// Alias para compatibilidad con la app Android que envía "frecuenciaCardiaca".
    /// Si se recibe este campo y Valor == 0, se usa como Valor con Tipo = FrecuenciaCardiaca.
    /// </summary>
    [JsonPropertyName("frecuenciaCardiaca")]
    public int? FrecuenciaCardiacaCompat
    {
        get => null;
        set
        {
            if (value.HasValue && Valor == 0)
            {
                Valor = value.Value;
                Tipo = TipoMedicion.FrecuenciaCardiaca;
            }
        }
    }
}
