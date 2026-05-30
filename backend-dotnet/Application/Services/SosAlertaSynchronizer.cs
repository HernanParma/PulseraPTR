using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Mantiene alineados evento SOS y alerta generada en el mismo registro (mismo paciente y fecha/hora).
/// </summary>
internal static class SosAlertaSynchronizer
{
    private static readonly TimeSpan VentanaEmparejamiento = TimeSpan.FromSeconds(2);

    public static bool EsAlertaSos(TipoAlerta tipo) =>
        tipo is TipoAlerta.SosManual or TipoAlerta.SosAutomatico;

    public static async Task MarcarEventoSosAtendidoSiExisteAsync(
        IEventoEmergenciaRepository eventos,
        Alerta alerta,
        CancellationToken cancellationToken)
    {
        if (!EsAlertaSos(alerta.TipoAlerta))
            return;

        var evento = await eventos.GetSosPorPacienteYFechaAsync(
            alerta.PacienteId, alerta.FechaHora, VentanaEmparejamiento, cancellationToken);

        if (evento is null || evento.Atendido)
            return;

        evento.Atendido = true;
        eventos.Update(evento);
    }

    public static async Task MarcarAlertasSosLeidasSiExistenAsync(
        IAlertaRepository alertas,
        EventoEmergencia evento,
        CancellationToken cancellationToken)
    {
        if (evento.TipoEvento != TipoEventoEmergencia.SOS)
            return;

        var lista = await alertas.GetSosPorPacienteYFechaAsync(
            evento.PacienteId, evento.FechaHora, VentanaEmparejamiento, cancellationToken);

        foreach (var alerta in lista.Where(a => !a.Leida))
        {
            alerta.Leida = true;
            alertas.Update(alerta);
        }
    }
}
