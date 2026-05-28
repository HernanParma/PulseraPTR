using Domain.Enums;

namespace Application.Dtos;

public class MedicionDto
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public string? PacienteNombre { get; set; }
    public DateTime FechaHora { get; set; }
    public int FrecuenciaCardiaca { get; set; }
    public int? PasosActividad { get; set; }
    public int? NivelEstres { get; set; }
    public int? MinutosSueno { get; set; }
    public int? MinutosActividad { get; set; }
    public int? CaloriasQuemadas { get; set; }
    public EstadoClinico Estado { get; set; }
    public string? MensajeAlerta { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
    public bool EsFueraDeRango { get; set; }
}
