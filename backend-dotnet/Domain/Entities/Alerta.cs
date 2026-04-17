using Domain.Enums;

namespace Domain.Entities;

public class Alerta
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public TipoAlerta TipoAlerta { get; set; }
    public EstadoClinico Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }

    public Paciente Paciente { get; set; } = null!;
}
