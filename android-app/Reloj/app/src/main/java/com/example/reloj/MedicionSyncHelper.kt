package com.example.reloj

import android.content.Context
import androidx.health.connect.client.HealthConnectClient
import com.pulseraptr.network.NetworkingConfig
import java.time.Instant

data class ResultadoSincronizacion(
    val exitoLectura: Boolean,
    val enviadoAlBackend: Boolean,
    val omitidoPorIntervalo: Boolean,
    val bpm: Long?,
    val pasosHoy: Int?,
    val metricas: MetricasSalud,
    val estado: String?,
    val mensajeAlerta: String?,
    val mensajeBackend: String?,
    val error: String?,
)

object MedicionSyncHelper {

    private const val PREFS = "pulsera_medicion_sync"
    private const val KEY_LAST_SENT_MS = "last_sent_ms"
    private const val KEY_LAST_READ_MS = "last_read_ms"
    private const val KEY_SNAP_PASOS = "snap_pasos"
    private const val KEY_SNAP_ESTRES = "snap_estres"
    private const val KEY_SNAP_SUENO = "snap_sueno"
    private const val KEY_SNAP_ACT = "snap_act"
    private const val KEY_SNAP_KCAL = "snap_kcal"
    private val margenDedupMs = (NetworkingConfig.INTERVALO_ENVIO_AUTOMATICO_MS * 0.85).toLong()

    private fun prefs(context: Context) =
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)

    private fun marcarLectura(context: Context) {
        prefs(context).edit().putLong(KEY_LAST_READ_MS, System.currentTimeMillis()).apply()
    }

    private fun cargarMetricasAnteriores(context: Context): MetricasSalud {
        val p = prefs(context)
        if (!p.contains(KEY_SNAP_ESTRES) && !p.contains(KEY_SNAP_SUENO)
            && !p.contains(KEY_SNAP_ACT) && !p.contains(KEY_SNAP_KCAL)
        ) {
            return MetricasSalud()
        }
        return MetricasSalud(
            nivelEstres = p.getInt(KEY_SNAP_ESTRES, -1).takeIf { it >= 0 },
            minutosSueno = p.getInt(KEY_SNAP_SUENO, -1).takeIf { it >= 0 },
            minutosActividad = p.getInt(KEY_SNAP_ACT, -1).takeIf { it >= 0 },
            caloriasQuemadas = p.getInt(KEY_SNAP_KCAL, -1).takeIf { it >= 0 },
        )
    }

    private fun guardarSnapshotEnvio(
        context: Context,
        pasos: Int?,
        metricas: MetricasSalud,
    ) {
        prefs(context).edit().apply {
            if (pasos != null) putInt(KEY_SNAP_PASOS, pasos) else remove(KEY_SNAP_PASOS)
            metricas.nivelEstres?.let { putInt(KEY_SNAP_ESTRES, it) } ?: remove(KEY_SNAP_ESTRES)
            metricas.minutosSueno?.let { putInt(KEY_SNAP_SUENO, it) } ?: remove(KEY_SNAP_SUENO)
            metricas.minutosActividad?.let { putInt(KEY_SNAP_ACT, it) } ?: remove(KEY_SNAP_ACT)
            metricas.caloriasQuemadas?.let { putInt(KEY_SNAP_KCAL, it) } ?: remove(KEY_SNAP_KCAL)
        }.apply()
    }

    fun marcarEnviado(context: Context) {
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .putLong(KEY_LAST_SENT_MS, System.currentTimeMillis())
            .apply()
    }

    private fun puedeEnviarAutomatico(context: Context): Boolean {
        val last = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .getLong(KEY_LAST_SENT_MS, 0L)
        if (last == 0L) return true
        return System.currentTimeMillis() - last >= margenDedupMs
    }

    suspend fun sincronizar(
        context: Context,
        client: HealthConnectClient,
        permisosConcedidos: Boolean,
        forzarEnvio: Boolean,
    ): ResultadoSincronizacion {
        if (!permisosConcedidos) {
            return ResultadoSincronizacion(
                exitoLectura = false,
                enviadoAlBackend = false,
                omitidoPorIntervalo = false,
                bpm = null,
                pasosHoy = null,
                metricas = MetricasSalud(),
                estado = null,
                mensajeAlerta = null,
                mensajeBackend = null,
                error = "Permisos Health Connect incompletos",
            )
        }

        if (!forzarEnvio && !puedeEnviarAutomatico(context)) {
            return ResultadoSincronizacion(
                exitoLectura = false,
                enviadoAlBackend = false,
                omitidoPorIntervalo = true,
                bpm = null,
                pasosHoy = null,
                metricas = MetricasSalud(),
                estado = null,
                mensajeAlerta = null,
                mensajeBackend = null,
                error = null,
            )
        }

        return try {
            val hasta = Instant.now()
            val ultimaLecturaMs = prefs(context).getLong(KEY_LAST_READ_MS, 0L)
            val desde = HealthConnectLectura.instanteInicioVentana(ultimaLecturaMs, hasta.toEpochMilli())

            val lecturaBase = HealthConnectLectura.leerEnVentana(client, desde, hasta)
            val lectura = HealthConnectLectura.completarParaEnvio(client, lecturaBase, hasta)
            marcarLectura(context)

            val bpm = lectura.bpm
            if (bpm == null) {
                return ResultadoSincronizacion(
                    exitoLectura = false,
                    enviadoAlBackend = false,
                    omitidoPorIntervalo = false,
                    bpm = null,
                    pasosHoy = null,
                    metricas = MetricasSalud(),
                    estado = null,
                    mensajeAlerta = null,
                    mensajeBackend = null,
                    error = "Sin FC nueva en Health Connect (sync del reloj)",
                )
            }

            val snapshotAnterior = cargarMetricasAnteriores(context)
            val pasosSnap = prefs(context).getInt(KEY_SNAP_PASOS, -1).takeIf { it >= 0 }
            // Pasos = máximo acumulado del día (Samsung), nunca suma del período.
            val pasosParaEnvio = lectura.pasosMaxHoy ?: pasosSnap
            val metricasEnvio = lectura.metricas
                .combinarConAnterior(snapshotAnterior)
                .conEstresSimuladoSiFalta()

            val (ok, mensajeBackend) = enviarMedicionAlBackend(
                frecuenciaCardiaca = bpm,
                pasosActividad = pasosParaEnvio,
                metricas = metricasEnvio,
            )

            if (ok) {
                marcarEnviado(context)
                guardarSnapshotEnvio(context, pasosParaEnvio, metricasEnvio)
            }

            ResultadoSincronizacion(
                exitoLectura = true,
                enviadoAlBackend = ok,
                omitidoPorIntervalo = false,
                bpm = bpm,
                pasosHoy = pasosParaEnvio,
                metricas = metricasEnvio,
                estado = null,
                mensajeAlerta = null,
                mensajeBackend = mensajeBackend,
                error = if (ok) null else mensajeBackend,
            )
        } catch (e: Exception) {
            ResultadoSincronizacion(
                exitoLectura = false,
                enviadoAlBackend = false,
                omitidoPorIntervalo = false,
                bpm = null,
                pasosHoy = null,
                metricas = MetricasSalud(),
                estado = null,
                mensajeAlerta = null,
                mensajeBackend = null,
                error = e.message ?: "Error leyendo Health Connect",
            )
        }
    }

    fun aplicarResultadoEnUi(
        resultado: ResultadoSincronizacion,
        onHeartRateText: (String) -> Unit,
        onPasosText: (String) -> Unit,
        onEstresText: (String) -> Unit,
        onSuenoText: (String) -> Unit,
        onActividadText: (String) -> Unit,
        onCaloriasText: (String) -> Unit,
        onEstado: (String) -> Unit,
        onAlerta: (String) -> Unit,
        onUltimoEvento: (String) -> Unit,
    ) {
        if (resultado.omitidoPorIntervalo) return

        val bpm = resultado.bpm
        if (bpm != null) {
            onHeartRateText("$bpm bpm")
            onPasosText(
                resultado.pasosHoy?.let { "$it pasos" } ?: "Sin dato de pasos"
            )
            onEstresText(
                resultado.metricas.nivelEstres?.let { "$it / 100 (simulado)" }
                    ?: "Sin dato de estrés"
            )
            onSuenoText(
                resultado.metricas.minutosSueno?.let { "${it / 60}h ${it % 60}m" }
                    ?: "Sin dato de sueño"
            )
            onActividadText(
                resultado.metricas.minutosActividad?.let { "$it min actividad" }
                    ?: "Sin dato de actividad"
            )
            onCaloriasText(
                resultado.metricas.caloriasQuemadas?.let { "$it kcal" } ?: "Sin dato de calorías"
            )
        }

        resultado.error?.let {
            if (bpm == null) {
                onHeartRateText("Error al leer Health Connect")
            }
            onUltimoEvento(it)
            return
        }

        onUltimoEvento(
            if (resultado.enviadoAlBackend) "Lectura enviada al backend"
            else "Lectura OK; falló envío al backend"
        )
    }
}
