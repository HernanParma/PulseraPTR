using Domain.Enums;

namespace Application.Dtos;

/// <summary>
/// Fila unificada para la tabla de Mediciones (pulsera + glucemia mySugr).
/// </summary>
public sealed class MedicionListadoFilaDto
{
    public MedicionListadoTipo Tipo { get; init; }
    public int Id { get; init; }
    public DateTime FechaHora { get; init; }
    public string? PacienteNombre { get; init; }
    public int? FrecuenciaCardiaca { get; init; }
    public int? PasosActividad { get; init; }
    public int? NivelEstres { get; init; }
    public int? MinutosSueno { get; init; }
    public int? MinutosActividad { get; init; }
    public int? CaloriasQuemadas { get; init; }
    public int? GlucemiaMgDl { get; init; }
    public EstadoClinico? Estado { get; init; }
    public bool EsFueraDeRango { get; init; }
    public string Origen { get; init; } = string.Empty;
    public string? Mensaje { get; init; }
    public bool PuedeEliminar { get; init; }
}

public enum MedicionListadoTipo
{
    Pulsera = 0,
    Glucemia = 1
}
