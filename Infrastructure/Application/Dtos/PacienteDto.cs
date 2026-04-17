using Domain.Enums;

namespace Application.Dtos;

public class PacienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string? Dni { get; set; }
    public string ContactoEmergencia { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
    public EstadoClinico? EstadoActual { get; set; }
    public DateTime? UltimaMedicionFechaHora { get; set; }
}
