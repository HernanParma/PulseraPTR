using Domain.Enums;

namespace Application.Dtos;

public class CrearMedicionDto
{
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int Valor { get; set; }
    public TipoMedicion Tipo { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
}
