using Domain.Enums;

namespace Application.Dtos;

public class MedicionDto
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public string? PacienteNombre { get; set; }
    public DateTime FechaHora { get; set; }
    public int FrecuenciaCardiaca { get; set; }
    public EstadoClinico Estado { get; set; }
    public string? MensajeAlerta { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
    public bool EsFueraDeRango { get; set; }
}
