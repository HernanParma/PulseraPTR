namespace Application.Dtos;

public class DashboardResumenDto
{
    public int CantidadPacientesActivos { get; set; }
    public int CantidadAlertasActivas { get; set; }
    public int CantidadEventosSOSPendientes { get; set; }
    public int CantidadMedicionesFueraDeRango { get; set; }
    public int CantidadGlucemiasFueraDeRango { get; set; }
    public IReadOnlyList<EstadoPacienteResumenDto> EstadoActualPorPaciente { get; set; } = Array.Empty<EstadoPacienteResumenDto>();
    public IReadOnlyList<AlertaDto> UltimasAlertas { get; set; } = Array.Empty<AlertaDto>();
    public IReadOnlyList<EventoEmergenciaDto> UltimosEventosSos { get; set; } = Array.Empty<EventoEmergenciaDto>();
    public IReadOnlyList<GlucemiaFueraDeRangoResumenDto> UltimasGlucemiasFueraDeRango { get; set; } = Array.Empty<GlucemiaFueraDeRangoResumenDto>();
}

public class EstadoPacienteResumenDto
{
    public int PacienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? EstadoActual { get; set; }
    public DateTime? UltimaMedicionFechaHora { get; set; }
}

public class GlucemiaFueraDeRangoResumenDto
{
    public int PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public int GlucosaMgDl { get; set; }
    public string Banda { get; set; } = string.Empty;
    public DateTime FechaHoraUtc { get; set; }

    public string BandaTexto =>
        Banda switch
        {
            "Hyperglycemia" => "Hiperglucemia",
            "SevereHyperglycemia" => "Hiperglucemia Severa",
            "Hypoglycemia" => "Hipoglucemia",
            "SevereHypoglycemia" => "Hipoglucemia Severa",
            "CriticalIsolated" => "Crítico",
            _ => Banda
        };
}
