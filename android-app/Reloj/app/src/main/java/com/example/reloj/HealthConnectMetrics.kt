package com.example.reloj

import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.records.ActiveCaloriesBurnedRecord
import androidx.health.connect.client.records.ExerciseSessionRecord
import androidx.health.connect.client.records.HeartRateVariabilityRmssdRecord
import androidx.health.connect.client.records.SleepSessionRecord
import androidx.health.connect.client.records.TotalCaloriesBurnedRecord
import androidx.health.connect.client.request.ReadRecordsRequest
import androidx.health.connect.client.time.TimeRangeFilter
import java.time.Instant
import java.time.temporal.ChronoUnit
import kotlin.math.roundToInt

/** Métricas adicionales leídas de Health Connect (Samsung Health → Galaxy Watch). */
data class MetricasSalud(
    val nivelEstres: Int? = null,
    val minutosSueno: Int? = null,
    val minutosActividad: Int? = null,
    val caloriasQuemadas: Int? = null,
)

/**
 * Estrés directo de Samsung Health no se exporta a Health Connect.
 * Usamos HRV (RMSSD) como aproximación cuando el reloj lo sincroniza.
 */
fun hrvRmssdAEstresAproximado(rmssdMillis: Double): Int {
    val nivel = (100.0 - rmssdMillis * 1.15).roundToInt()
    return nivel.coerceIn(12, 92)
}

/** Samsung no exporta estrés; mismo rango que el backend (20–80). */
fun estresSimulado(): Int = (20..80).random()

fun MetricasSalud.conEstresSimuladoSiFalta(): MetricasSalud =
    if (nivelEstres != null) this else copy(nivelEstres = estresSimulado())

suspend fun leerMetricasAdicionales(
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
            ReadRecordsRequest(recordType = HeartRateVariabilityRmssdRecord::class, timeRangeFilter = filtro)
        )
        val ultimoHrv = hrv.records.maxByOrNull { it.time }
        if (ultimoHrv != null) {
            nivelEstres = hrvRmssdAEstresAproximado(ultimoHrv.heartRateVariabilityMillis)
        }
    } catch (_: Exception) { /* permiso o tipo no disponible */ }

    try {
        val sueno = client.readRecords(
            ReadRecordsRequest(recordType = SleepSessionRecord::class, timeRangeFilter = filtro)
        )
        val ultimaSesion = sueno.records.maxByOrNull { it.endTime }
        if (ultimaSesion != null) {
            minutosSueno = ChronoUnit.MINUTES.between(ultimaSesion.startTime, ultimaSesion.endTime)
                .toInt()
                .coerceAtLeast(0)
        }
    } catch (_: Exception) { }

    try {
        val ejercicio = client.readRecords(
            ReadRecordsRequest(recordType = ExerciseSessionRecord::class, timeRangeFilter = filtro)
        )
        val totalMin = ejercicio.records.sumOf { sesion ->
            ChronoUnit.MINUTES.between(sesion.startTime, sesion.endTime).coerceAtLeast(0)
        }
        if (totalMin > 0) minutosActividad = totalMin.toInt()
    } catch (_: Exception) { }

    try {
        var kcal = 0.0
        val activas = client.readRecords(
            ReadRecordsRequest(recordType = ActiveCaloriesBurnedRecord::class, timeRangeFilter = filtro)
        )
        kcal += activas.records.sumOf { it.energy.inKilocalories }
        if (kcal <= 0.0) {
            val totales = client.readRecords(
                ReadRecordsRequest(recordType = TotalCaloriesBurnedRecord::class, timeRangeFilter = filtro)
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
