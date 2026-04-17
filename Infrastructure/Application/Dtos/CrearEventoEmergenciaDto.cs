using Domain.Enums;

namespace Application.Dtos;

public class CrearEventoEmergenciaDto
{
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public TipoEventoEmergencia TipoEvento { get; set; } = TipoEventoEmergencia.SOS;
    public EstadoClinico Estado { get; set; } = EstadoClinico.CRITICO;
    public string Mensaje { get; set; } = string.Empty;
}
