using Domain.Enums;

namespace Domain.Entities;

public class Medicion
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int FrecuenciaCardiaca { get; set; }
    public EstadoClinico Estado { get; set; }
    public string? MensajeAlerta { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
    public bool EsFueraDeRango { get; set; }

    public Paciente Paciente { get; set; } = null!;
}
