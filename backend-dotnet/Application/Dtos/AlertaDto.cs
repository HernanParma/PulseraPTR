using Domain.Enums;

namespace Application.Dtos;

public class AlertaDto
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public string? PacienteNombre { get; set; }
    public DateTime FechaHora { get; set; }
    public TipoAlerta TipoAlerta { get; set; }
    public EstadoClinico Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
}
