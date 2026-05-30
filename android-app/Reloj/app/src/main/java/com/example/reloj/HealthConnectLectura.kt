package com.example.reloj

import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.records.HeartRateRecord
import androidx.health.connect.client.records.StepsRecord
import androidx.health.connect.client.request.ReadRecordsRequest
import androidx.health.connect.client.time.TimeRangeFilter
import com.pulseraptr.network.NetworkingConfig
import java.time.Instant
import java.time.temporal.ChronoUnit
import kotlin.math.roundToInt

/**
 * Lectura de métricas en la ventana desde el último envío (p. ej. 10 min),
 * para que cada medición automática refleje datos nuevos del período.
 */
data class LecturaSaludCompleta(
    val bpm: Long?,
    val pasosMaxHoy: Int?,
    val metricas: MetricasSalud,
    val resumenVentana: String,
)

object HealthConnectLectura {

    private val ventanaLecturaMinutos: Long
        get() = NetworkingConfig.INTERVALO_ENVIO_AUTOMATICO_MINUTOS + 2

    suspend fun leerEnVentana(
        client: HealthConnectClient,
        desde: Instant,
        hasta: Instant,
    ): LecturaSaludCompleta {
        val bpm = leerFrecuenciaCardiaca(client, desde, hasta)
        val pasosMaxHoy = leerPasosMaxHoy(client, hasta)
        val metricas = leerMetricasEnVentana(client, desde, hasta)

        val min = ChronoUnit.MINUTES.between(desde, hasta).coerceAtLeast(1)
        val resumen = buildString {
            append("Ventana ${min} min")
            bpm?.let { append(" · FC $it") }
            pasosMaxHoy?.takeIf { it > 0 }?.let { append(" · $it pasos (día)") }
            metricas.minutosActividad?.takeIf { it > 0 }?.let { append(" · ${it} min act.") }
            metricas.caloriasQuemadas?.takeIf { it > 0 }?.let { append(" · ${it} kcal") }
        }

        return LecturaSaludCompleta(
            bpm = bpm,
            pasosMaxHoy = pasosMaxHoy,
            metricas = metricas,
            resumenVentana = resumen,
        )
    }

    /** Si el período no trae métricas nuevas, completa con lo último disponible (24 h). */
    suspend fun completarParaEnvio(
        client: HealthConnectClient,
        lectura: LecturaSaludCompleta,
        hasta: Instant,
    ): LecturaSaludCompleta {
        var metricas = lectura.metricas
        if (metricas.estaVacia()) {
            val desde24h = hasta.minus(24, ChronoUnit.HOURS)
            metricas = leerMetricasAdicionales(client, desde24h, hasta)
        }
        return lectura.copy(metricas = metricas)
    }

    fun instanteInicioVentana(ultimaLecturaMs: Long, ahoraMs: Long = System.currentTimeMillis()): Instant {
        val ventanaMs = ventanaLecturaMinutos * 60_000
        val desdeMs = if (ultimaLecturaMs > 0) {
            maxOf(ultimaLecturaMs, ahoraMs - ventanaMs)
        } else {
            ahoraMs - ventanaMs
        }
        return Instant.ofEpochMilli(desdeMs)
    }

    private suspend fun leerFrecuenciaCardiaca(
        client: HealthConnectClient,
        desde: Instant,
        hasta: Instant,
    ): Long? {
        val enVentana = muestrasFc(client, desde, hasta)
            .maxByOrNull { it.time }
            ?.beatsPerMinute
            ?.toLong()

        if (enVentana != null) return enVentana

        // Sin muestras nuevas en el período: última FC de las últimas 2 h
        val fallbackDesde = hasta.minus(2, ChronoUnit.HOURS)
        return muestrasFc(client, fallbackDesde, hasta)
            .maxByOrNull { it.time }
            ?.beatsPerMinute
            ?.toLong()
    }

    private suspend fun muestrasFc(
        client: HealthConnectClient,
        desde: Instant,
        hasta: Instant,
    ) = client.readRecords(
        ReadRecordsRequest(
            recordType = HeartRateRecord::class,
            timeRangeFilter = TimeRangeFilter.between(desde, hasta),
        )
    ).records.flatMap { it.samples }.filter { sample ->
        !sample.time.isBefore(desde) && !sample.time.isAfter(hasta)
    }

    private suspend fun leerPasosMaxHoy(client: HealthConnectClient, hasta: Instant): Int? {
        val inicioHoy = java.time.LocalDate.now()
            .atStartOfDay(java.time.ZoneId.systemDefault())
            .toInstant()
        val records = client.readRecords(
            ReadRecordsRequest(
                recordType = StepsRecord::class,
                timeRangeFilter = TimeRangeFilter.between(inicioHoy, hasta),
            )
        ).records
        return records.maxOfOrNull { it.count }?.toInt()
    }

    private suspend fun leerMetricasEnVentana(
        client: HealthConnectClient,
        desde: Instant,
        hasta: Instant,
    ): MetricasSalud {
        val filtro = TimeRangeFilter.between(desde, hasta)

        var nivelEstres: Int? = null
        var minutosSueno: Int? = null
        var minutosActividad: Int? = null
        var caloriasQuemadas: Int? = null

        try {
            val hrv = client.readRecords(
                ReadRecordsRequest(
                    recordType = androidx.health.connect.client.records.HeartRateVariabilityRmssdRecord::class,
                    timeRangeFilter = filtro,
                )
            )
            hrv.records.maxByOrNull { it.time }?.let {
                nivelEstres = hrvRmssdAEstresAproximado(it.heartRateVariabilityMillis)
            }
        } catch (_: Exception) { }

        try {
            val sueno = client.readRecords(
                ReadRecordsRequest(
                    recordType = androidx.health.connect.client.records.SleepSessionRecord::class,
                    timeRangeFilter = filtro,
                )
            )
            // Solo sueño si hubo sesión que terminó en este período (evita repetir la misma noche)
            sueno.records
                .filter { it.endTime >= desde && it.endTime <= hasta }
                .maxByOrNull { it.endTime }
                ?.let { sesion ->
                    minutosSueno = ChronoUnit.MINUTES.between(sesion.startTime, sesion.endTime)
                        .toInt()
                        .coerceAtLeast(0)
                }
        } catch (_: Exception) { }

        try {
            val ejercicio = client.readRecords(
                ReadRecordsRequest(
                    recordType = androidx.health.connect.client.records.ExerciseSessionRecord::class,
                    timeRangeFilter = filtro,
                )
            )
            val totalMin = ejercicio.records
                .filter { it.endTime >= desde && it.startTime <= hasta }
                .sumOf { sesion ->
                    val inicio = maxOf(sesion.startTime, desde)
                    val fin = minOf(sesion.endTime, hasta)
                    ChronoUnit.MINUTES.between(inicio, fin).coerceAtLeast(0)
                }
            if (totalMin > 0) minutosActividad = totalMin.toInt()
        } catch (_: Exception) { }

        try {
            var kcal = 0.0
            val activas = client.readRecords(
                ReadRecordsRequest(
                    recordType = androidx.health.connect.client.records.ActiveCaloriesBurnedRecord::class,
                    timeRangeFilter = filtro,
                )
            )
            kcal += activas.records.sumOf { it.energy.inKilocalories }
            if (kcal <= 0.0) {
                val totales = client.readRecords(
                    ReadRecordsRequest(
                        recordType = androidx.health.connect.client.records.TotalCaloriesBurnedRecord::class,
                        timeRangeFilter = filtro,
                    )
                )
                kcal = totales.records.sumOf { it.energy.inKilocalories }
            }
            if (kcal > 0.0) caloriasQuemadas = kcal.roundToInt()
        } catch (_: Exception) { }

        return MetricasSalud(
            nivelEstres = nivelEstres,
            minutosSueno = minutosSueno,
            minutosActividad = minutosActividad,
            caloriasQuemadas = caloriasQuemadas,
        )
    }
}

fun MetricasSalud.estaVacia(): Boolean =
    nivelEstres == null && minutosSueno == null && minutosActividad == null && caloriasQuemadas == null

fun MetricasSalud.combinarConAnterior(anterior: MetricasSalud?): MetricasSalud {
    if (anterior == null) return this
    return MetricasSalud(
        nivelEstres = nivelEstres ?: anterior.nivelEstres,
        minutosSueno = minutosSueno ?: anterior.minutosSueno,
        minutosActividad = minutosActividad ?: anterior.minutosActividad,
        caloriasQuemadas = caloriasQuemadas ?: anterior.caloriasQuemadas,
    )
}
