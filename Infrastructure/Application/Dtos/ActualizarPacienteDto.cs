using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class ActualizarPacienteDto
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 130)]
    public int Edad { get; set; }

    [StringLength(32)]
    public string? Dni { get; set; }

    [Required, StringLength(200)]
    public string ContactoEmergencia { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Observaciones { get; set; }
}
