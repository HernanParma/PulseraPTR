using Application.Dtos.Glucose;

namespace Application.Dtos;

public class PacienteDetalleDto : PacienteDto
{
    public IReadOnlyList<MedicionDto> Mediciones { get; set; } = Array.Empty<MedicionDto>();
    public IReadOnlyList<AlertaDto> Alertas { get; set; } = Array.Empty<AlertaDto>();
    public IReadOnlyList<EventoEmergenciaDto> EventosSos { get; set; } = Array.Empty<EventoEmergenciaDto>();
    public GlucoseDashboardDto? Glucemia { get; set; }
}
