using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mapping;
using Domain.Enums;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IPacienteRepository _pacientes;
    private readonly IMedicionRepository _mediciones;
    private readonly IAlertaRepository _alertas;
    private readonly IEventoEmergenciaRepository _eventos;
    private readonly IPacienteService _pacienteService;

    public DashboardService(
        IPacienteRepository pacientes,
        IMedicionRepository mediciones,
        IAlertaRepository alertas,
        IEventoEmergenciaRepository eventos,
        IPacienteService pacienteService)
    {
        _pacientes = pacientes;
        _mediciones = mediciones;
        _alertas = alertas;
        _eventos = eventos;
        _pacienteService = pacienteService;
    }

    public async Task<DashboardResumenDto> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        var activos = await _pacientes.ContarActivosAsync(cancellationToken);
        var alertasActivas = await _alertas.ContarActivasAsync(cancellationToken);
        var sosPendientes = await _eventos.ContarSosPendientesAsync(cancellationToken);
        var fueraDeRango = await _mediciones.ContarFueraDeRangoAsync(cancellationToken);

        var pacientesActivos = await _pacientes.GetAllAsync(incluirInactivos: false, cancellationToken);
        var estadoPorPaciente = new List<EstadoPacienteResumenDto>();

        foreach (var p in pacientesActivos)
        {
            var ultima = await _mediciones.GetUltimaPorPacienteAsync(p.Id, cancellationToken);
            estadoPorPaciente.Add(new EstadoPacienteResumenDto
            {
                PacienteId = p.Id,
                Nombre = p.Nombre,
                EstadoActual = ultima?.Estado.ToString(),
                UltimaMedicionFechaHora = ultima is null ? null : PulseraMapper.ParaVisualizacionFechaHora(ultima.FechaHora)
            });
        }

        var ultimasAlertas = await _alertas.GetUltimasAsync(10, cancellationToken);
        var ultimosSos = await _eventos.GetUltimosAsync(10, TipoEventoEmergencia.SOS, cancellationToken);

        return new DashboardResumenDto
        {
            CantidadPacientesActivos = activos,
            CantidadAlertasActivas = alertasActivas,
            CantidadEventosSOSPendientes = sosPendientes,
            CantidadMedicionesFueraDeRango = fueraDeRango,
            EstadoActualPorPaciente = estadoPorPaciente.OrderBy(e => e.Nombre).ToList(),
            UltimasAlertas = ultimasAlertas.Select(a => a.ToDto()).ToList(),
            UltimosEventosSos = ultimosSos.Select(e => e.ToDto()).ToList()
        };
    }

    public Task<PacienteDetalleDto> ObtenerDetallePacienteAsync(int pacienteId, CancellationToken cancellationToken = default) =>
        _pacienteService.ObtenerDetalleAsync(pacienteId, cancellationToken);
}
