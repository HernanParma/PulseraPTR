using Domain.Enums;

namespace Application.Dtos;

public class CrearMedicionDto
{
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int FrecuenciaCardiaca { get; set; }
    /// <summary>
    /// Opcional: el servidor reclasifica por FC; se conserva solo si se requiere trazabilidad del cliente.
    /// </summary>
    public EstadoClinico? Estado { get; set; }
    public string? MensajeAlerta { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
}
