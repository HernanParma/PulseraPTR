using Domain.Enums;

namespace Domain.Entities;

public class EventoEmergencia
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public TipoEventoEmergencia TipoEvento { get; set; }
    public EstadoClinico Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Atendido { get; set; }

    public Paciente Paciente { get; set; } = null!;
}
