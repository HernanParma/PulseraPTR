namespace Domain.Entities;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string? Dni { get; set; }
    public string ContactoEmergencia { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Medicion> Mediciones { get; set; } = new List<Medicion>();
    public ICollection<EventoEmergencia> EventosEmergencia { get; set; } = new List<EventoEmergencia>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    public ICollection<GlucoseReading> GlucoseReadings { get; set; } = new List<GlucoseReading>();
}
