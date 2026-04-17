namespace Application.Dtos;

public class DashboardResumenDto
{
    public int CantidadPacientesActivos { get; set; }
    public int CantidadAlertasActivas { get; set; }
    public int CantidadEventosSOSPendientes { get; set; }
    public int CantidadMedicionesFueraDeRango { get; set; }
    public IReadOnlyList<EstadoPacienteResumenDto> EstadoActualPorPaciente { get; set; } = Array.Empty<EstadoPacienteResumenDto>();
    public IReadOnlyList<AlertaDto> UltimasAlertas { get; set; } = Array.Empty<AlertaDto>();
    public IReadOnlyList<EventoEmergenciaDto> UltimosEventosSos { get; set; } = Array.Empty<EventoEmergenciaDto>();
}

public class EstadoPacienteResumenDto
{
    public int PacienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? EstadoActual { get; set; }
    public DateTime? UltimaMedicionFechaHora { get; set; }
}
