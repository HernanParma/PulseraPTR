using Application.Configuration;
using Application.Dtos;
using Application.Dtos.Glucose;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mapping;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Options;

namespace Application.Services;

public sealed class MedicionesListadoService : IMedicionesListadoService
{
    private readonly IMedicionRepository _mediciones;
    private readonly IGlucoseReadingRepository _glucose;
    private readonly GlucoseAlertOptions _glucoseThresholds;

    public MedicionesListadoService(
        IMedicionRepository mediciones,
        IGlucoseReadingRepository glucose,
        IOptions<GlucoseAlertOptions> glucoseThresholds)
    {
        _mediciones = mediciones;
        _glucose = glucose;
        _glucoseThresholds = glucoseThresholds.Value;
    }

    public async Task<MedicionesIndexDto> ObtenerIndexAsync(
        int? pacienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        EstadoClinico? estado,
        int pagina = 1,
        int tamanoPagina = MedicionesIndexDto.TamanoPaginaPorDefecto,
        CancellationToken cancellationToken = default)
    {
        var mediciones = await _mediciones.BuscarAsync(
            pacienteId, fechaDesde, fechaHasta, estado, null, cancellationToken);
        var medicionDtos = mediciones.Select(m => m.ToDto()).ToList();

        var glucoseEntities = await _glucose.BuscarAsync(
            pacienteId, fechaDesde, fechaHasta, cancellationToken);

        var filas = new List<MedicionListadoFilaDto>(medicionDtos.Count + glucoseEntities.Count);

        foreach (var m in medicionDtos)
        {
            filas.Add(new MedicionListadoFilaDto
            {
                Tipo = MedicionListadoTipo.Pulsera,
                Id = m.Id,
                FechaHora = m.FechaHora,
                PacienteNombre = m.PacienteNombre,
                FrecuenciaCardiaca = m.FrecuenciaCardiaca,
                PasosActividad = m.PasosActividad,
                NivelEstres = m.NivelEstres,
                MinutosSueno = m.MinutosSueno,
                MinutosActividad = m.MinutosActividad,
                CaloriasQuemadas = m.CaloriasQuemadas,
                Estado = m.Estado,
                EsFueraDeRango = m.EsFueraDeRango,
                Origen = m.OrigenDato,
                Mensaje = m.MensajeAlerta,
                PuedeEliminar = true
            });
        }

        AgregarFilasGlucemia(filas, glucoseEntities, estado);

        var ordenadas = DeduplicarPorPacienteYMismaHora(filas);
        var total = ordenadas.Count;
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);
        pagina = Math.Max(1, pagina);
        var totalPaginas = total == 0 ? 1 : (int)Math.Ceiling(total / (double)tamanoPagina);
        if (pagina > totalPaginas)
            pagina = totalPaginas;

        var paginaFilas = ordenadas
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        return new MedicionesIndexDto
        {
            MedicionesPulsera = medicionDtos,
            Filas = paginaFilas,
            TotalFilas = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalPaginas = totalPaginas
        };
    }

    /// <summary>
    /// Un correo/CSV = una fila en Mediciones (promedio de glucemias del lote, fecha = recepción del mail).
    /// Lecturas sin lote (histórico) se listan una por una.
    /// </summary>
    private void AgregarFilasGlucemia(
        List<MedicionListadoFilaDto> filas,
        IReadOnlyList<GlucoseReading> glucoseEntities,
        EstadoClinico? estado)
    {
        var conLote = glucoseEntities
            .Where(g => !string.IsNullOrWhiteSpace(g.ImportBatchId))
            .GroupBy(g => g.ImportBatchId!);

        foreach (var batch in conLote)
        {
            var readings = batch.ToList();
            if (readings.Count == 0)
                continue;

            var promedio = (int)Math.Round(readings.Average(r => r.GlucoseMgDl));
            var band = GlucoseReadingMapper.ClassifyBand(promedio, _glucoseThresholds);
            var estadoGlucemia = GlucoseReadingMapper.ToEstadoClinico(band);
            if (estado.HasValue && estadoGlucemia != estado.Value)
                continue;

            var refRow = readings.OrderByDescending(r => r.EmailReceivedAtUtc ?? r.CreatedAt).First();
            var fechaListado = refRow.EmailReceivedAtUtc.HasValue
                ? MySugrSpanishDateTimeParser.ConvertUtcToLocalForDisplay(
                    refRow.EmailReceivedAtUtc.Value, refRow.TimeZone)
                : refRow.CreatedAt;
            var esMail = refRow.ImportBatchId!.StartsWith("email-", StringComparison.OrdinalIgnoreCase);
            var origen = esMail ? "mySugr (mail)" : "mySugr (CSV)";

            filas.Add(new MedicionListadoFilaDto
            {
                Tipo = MedicionListadoTipo.Glucemia,
                Id = readings.Max(r => r.Id),
                FechaHora = fechaListado,
                PacienteNombre = refRow.Paciente?.Nombre,
                GlucemiaMgDl = promedio,
                Estado = estadoGlucemia,
                EsFueraDeRango = band != GlucoseRangeBand.Normal,
                Origen = origen,
                Mensaje = $"Importación · {readings.Count} lecturas en CSV · promedio {promedio} mg/dL ({band})",
                PuedeEliminar = false
            });
        }

        foreach (var g in glucoseEntities.Where(g => string.IsNullOrWhiteSpace(g.ImportBatchId)))
        {
            var dto = GlucoseReadingMapper.ToDto(g, _glucoseThresholds);
            var estadoGlucemia = GlucoseReadingMapper.ToEstadoClinico(dto.Band);
            if (estado.HasValue && estadoGlucemia != estado.Value)
                continue;

            filas.Add(new MedicionListadoFilaDto
            {
                Tipo = MedicionListadoTipo.Glucemia,
                Id = dto.Id,
                FechaHora = dto.ReadingDateTimeUtc,
                PacienteNombre = g.Paciente?.Nombre,
                GlucemiaMgDl = dto.GlucoseMgDl,
                Estado = estadoGlucemia,
                EsFueraDeRango = dto.Band != GlucoseRangeBand.Normal,
                Origen = g.Source == GlucoseReadingSource.MySugrCsvImport
                    ? "mySugr (histórico)"
                    : g.Source.ToString(),
                Mensaje = BuildGlucoseMensaje(dto),
                PuedeEliminar = false
            });
        }
    }

    /// <summary>
    /// Un registro por paciente, tipo y minuto (evita duplicados del envío automático de la APK).
    /// </summary>
    private static List<MedicionListadoFilaDto> DeduplicarPorPacienteYMismaHora(IEnumerable<MedicionListadoFilaDto> filas) =>
        filas
            .GroupBy(f => (
                f.Tipo,
                Paciente: f.PacienteNombre?.Trim() ?? string.Empty,
                Minuto: new DateTime(
                    f.FechaHora.Year,
                    f.FechaHora.Month,
                    f.FechaHora.Day,
                    f.FechaHora.Hour,
                    f.FechaHora.Minute,
                    0)))
            .Select(g => g.OrderByDescending(x => x.Id).First())
            .OrderByDescending(f => f.FechaHora)
            .ThenByDescending(f => f.Id)
            .ToList();

    private static string BuildGlucoseMensaje(GlucoseReadingDto dto)
    {
        var etiqueta = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim();
        var baseMsg = $"Glucemia {dto.GlucoseMgDl} mg/dL ({dto.Band})";
        return etiqueta is null ? baseMsg : $"{baseMsg} · {etiqueta}";
    }
}
