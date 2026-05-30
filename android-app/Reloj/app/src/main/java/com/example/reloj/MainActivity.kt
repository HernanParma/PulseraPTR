package com.example.reloj

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.PermissionController
import androidx.health.connect.client.permission.HealthPermission
import androidx.health.connect.client.records.ActiveCaloriesBurnedRecord
import androidx.health.connect.client.records.ExerciseSessionRecord
import androidx.health.connect.client.records.HeartRateRecord
import androidx.health.connect.client.records.HeartRateVariabilityRmssdRecord
import androidx.health.connect.client.records.SleepSessionRecord
import androidx.health.connect.client.records.StepsRecord
import androidx.health.connect.client.records.TotalCaloriesBurnedRecord
import com.pulseraptr.network.NetworkingConfig
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import java.util.concurrent.TimeUnit
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject
import java.text.SimpleDateFormat
import java.time.Instant
import java.util.Date
import java.util.Locale

private const val HEALTH_CONNECT_PACKAGE = "com.google.android.apps.healthdata"

private val httpClient by lazy {
    OkHttpClient.Builder()
        .connectTimeout(20, TimeUnit.SECONDS)
        .readTimeout(30, TimeUnit.SECONDS)
        .writeTimeout(30, TimeUnit.SECONDS)
        .build()
}

class MainActivity : ComponentActivity() {

    private lateinit var healthConnectClient: HealthConnectClient

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val sdkStatus = HealthConnectClient.getSdkStatus(this, HEALTH_CONNECT_PACKAGE)

        if (sdkStatus == HealthConnectClient.SDK_AVAILABLE) {
            healthConnectClient = HealthConnectClient.getOrCreate(this)
        }

        setContent {
            MaterialTheme {
                PantallaPrincipal(
                    sdkStatus = sdkStatus,
                    clientProvider = { healthConnectClient },
                    onOpenHealthConnect = {
                        val uri = Uri.parse("market://details?id=$HEALTH_CONNECT_PACKAGE")
                        startActivity(Intent(Intent.ACTION_VIEW, uri))
                    }
                )
            }
        }
    }
}

/** Umbrales alineados con el backend (rango normal 60–100 lpm, crítico fuera de 50–120). */
fun clasificarEstadoFrecuenciaCardiaca(bpm: Long): String {
    return when {
        bpm > 120 || bpm < 50 -> "CRITICO"
        bpm > 100 || bpm < 60 -> "ADVERTENCIA"
        else -> "NORMAL"
    }
}

fun generarMensajeAlerta(bpm: Long): String {
    return when {
        bpm > 120 -> "Frecuencia cardíaca críticamente alta"
        bpm < 50 -> "Frecuencia cardíaca críticamente baja"
        bpm > 100 -> "Frecuencia cardíaca alta"
        bpm < 60 -> "Frecuencia cardíaca baja"
        else -> "Frecuencia cardíaca normal"
    }
}

suspend fun enviarMedicionAlBackend(
    frecuenciaCardiaca: Long,
    pasosActividad: Int?,
    metricas: MetricasSalud = MetricasSalud(),
): Pair<Boolean, String> = withContext(Dispatchers.IO) {
    try {
        val url = "${NetworkingConfig.BASE_URL}api/mediciones"

        val json = JSONObject().apply {
            put("pacienteId", NetworkingConfig.DEFAULT_PACIENTE_ID)
            put("fechaHora", Instant.now().toString())
            put("frecuenciaCardiaca", frecuenciaCardiaca)
            if (pasosActividad != null) put("pasosActividad", pasosActividad)
            metricas.nivelEstres?.let { put("nivelEstres", it) }
            metricas.minutosSueno?.let { put("minutosSueno", it) }
            metricas.minutosActividad?.let { put("minutosActividad", it) }
            metricas.caloriasQuemadas?.let { put("caloriasQuemadas", it) }
            put("origenDato", "HealthConnect")
        }

        val body = json.toString()
            .toRequestBody("application/json; charset=utf-8".toMediaType())

        val request = Request.Builder()
            .url(url)
            .post(body)
            .build()

        httpClient.newCall(request).execute().use { response ->
            if (response.isSuccessful) {
                true to "Medición enviada al backend"
            } else {
                false to "Error HTTP ${response.code}"
            }
        }
    } catch (e: Exception) {
        false to "Error enviando medición: ${e.message ?: "desconocido"}"
    }
}

suspend fun enviarSosAlBackend(): Pair<Boolean, String> = withContext(Dispatchers.IO) {
    try {
        val url = "${NetworkingConfig.BASE_URL}api/eventos/sos"

        val json = JSONObject().apply {
            put("pacienteId", NetworkingConfig.DEFAULT_PACIENTE_ID)
            put("fechaHora", Instant.now().toString())
            put("tipoEvento", "SOS")
            put("estado", "CRITICO")
            put("mensaje", "Emergencia manual")
        }

        val body = json.toString()
            .toRequestBody("application/json; charset=utf-8".toMediaType())

        val request = Request.Builder()
            .url(url)
            .post(body)
            .build()

        httpClient.newCall(request).execute().use { response ->
            if (response.isSuccessful) {
                true to "SOS enviado al backend"
            } else {
                false to "Error HTTP ${response.code}"
            }
        }
    } catch (e: Exception) {
        false to "Error enviando SOS: ${e.message ?: "desconocido"}"
    }
}

private fun registrarSincronizacionEnHistorial(
    historial: MutableList<String>,
    horaActual: () -> String,
    resultado: ResultadoSincronizacion,
    manual: Boolean,
) {
    val prefijo = if (manual) "Manual" else "Auto"
    when {
        resultado.omitidoPorIntervalo -> { /* sin entrada */ }
        resultado.error != null -> {
            historial.add(0, "${horaActual()} - $prefijo: ${resultado.error}")
        }
        resultado.bpm != null -> {
            val m = resultado.metricas
            historial.add(
                0,
                "${horaActual()} - $prefijo FC: ${resultado.bpm} / Pasos: ${resultado.pasosHoy ?: "—"} / " +
                    "Estrés: ${m.nivelEstres ?: "—"} / Sueño: ${m.minutosSueno ?: "—"} min / Act: ${m.minutosActividad ?: "—"} min",
            )
            val msg = resultado.mensajeBackend ?: "sin respuesta"
            historial.add(0, "${horaActual()} - ${if (resultado.enviadoAlBackend) "✓" else "✗"} $msg")
        }
    }
}

private suspend fun ejecutarSincronizacionUi(
    context: android.content.Context,
    client: HealthConnectClient,
    permisosOk: Boolean,
    forzarEnvio: Boolean,
    manual: Boolean,
    horaActual: () -> String,
    historial: MutableList<String>,
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
    val resultado = MedicionSyncHelper.sincronizar(
        context = context,
        client = client,
        permisosConcedidos = permisosOk,
        forzarEnvio = forzarEnvio,
    )
    MedicionSyncHelper.aplicarResultadoEnUi(
        resultado,
        onHeartRateText,
        onPasosText,
        onEstresText,
        onSuenoText,
        onActividadText,
        onCaloriasText,
        onEstado,
        onAlerta,
        onUltimoEvento,
    )
    registrarSincronizacionEnHistorial(historial, horaActual, resultado, manual)
}

@Composable
fun PantallaPrincipal(
    sdkStatus: Int,
    clientProvider: () -> HealthConnectClient,
    onOpenHealthConnect: () -> Unit
) {
    val scope = rememberCoroutineScope()
    val appContext = LocalContext.current.applicationContext

    var estado by remember { mutableStateOf("NORMAL") }
    var ultimoEvento by remember { mutableStateOf("Sin eventos") }
    var heartRateText by remember { mutableStateOf("Sin lectura todavía") }
    var pasosText by remember { mutableStateOf("Sin lectura todavía") }
    var estresText by remember { mutableStateOf("Sin lectura todavía") }
    var suenoText by remember { mutableStateOf("Sin lectura todavía") }
    var actividadText by remember { mutableStateOf("Sin lectura todavía") }
    var caloriasText by remember { mutableStateOf("Sin lectura todavía") }
    var alertaActual by remember { mutableStateOf("Sin alertas") }

    val historial = remember { mutableStateListOf<String>() }

    fun horaActual(): String {
        return SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())
    }

    val permissions = setOf(
        HealthPermission.getReadPermission(HeartRateRecord::class),
        HealthPermission.getReadPermission(StepsRecord::class),
        HealthPermission.getReadPermission(SleepSessionRecord::class),
        HealthPermission.getReadPermission(ExerciseSessionRecord::class),
        HealthPermission.getReadPermission(ActiveCaloriesBurnedRecord::class),
        HealthPermission.getReadPermission(TotalCaloriesBurnedRecord::class),
        HealthPermission.getReadPermission(HeartRateVariabilityRmssdRecord::class),
    )

    var permisosFcListos by remember { mutableStateOf(false) }

    val permissionLauncher =
        rememberLauncherForActivityResult(
            PermissionController.createRequestPermissionResultContract()
        ) { granted ->
            permisosFcListos = permissions.all { it in granted }
            if (permisosFcListos) {
                ultimoEvento = "Permisos Health Connect otorgados"
                historial.add(0, "${horaActual()} - Permisos Health Connect otorgados (FC, pasos, sueño, ejercicio, calorías, HRV)")
                MedicionEnvioScheduler.iniciarCadena(appContext)
            } else {
                ultimoEvento = "Permiso denegado"
                historial.add(0, "${horaActual()} - Permiso Health Connect denegado")
            }
        }

    /** Al abrir: si ya hay permisos, marcar listo; si no, pedirlos solos (sin pulsar botón). */
    LaunchedEffect(sdkStatus) {
        if (sdkStatus != HealthConnectClient.SDK_AVAILABLE) return@LaunchedEffect
        delay(400)
        val client = clientProvider()
        val granted = client.permissionController.getGrantedPermissions()
        if (permissions.all { it in granted }) {
            permisosFcListos = true
            MedicionEnvioScheduler.iniciarCadena(appContext)
        } else {
            permissionLauncher.launch(permissions)
        }
    }

    /** Con permisos: envío automático cada 10 min mientras la app está abierta. */
    LaunchedEffect(permisosFcListos, sdkStatus) {
        if (sdkStatus != HealthConnectClient.SDK_AVAILABLE || !permisosFcListos) return@LaunchedEffect
        val client = clientProvider()
        while (isActive) {
            ejecutarSincronizacionUi(
                context = appContext,
                client = client,
                permisosOk = true,
                forzarEnvio = false,
                manual = false,
                horaActual = ::horaActual,
                historial = historial,
                onHeartRateText = { heartRateText = it },
                onPasosText = { pasosText = it },
                onEstresText = { estresText = it },
                onSuenoText = { suenoText = it },
                onActividadText = { actividadText = it },
                onCaloriasText = { caloriasText = it },
                onEstado = { estado = it },
                onAlerta = { alertaActual = it },
                onUltimoEvento = { ultimoEvento = it },
            )
            delay(NetworkingConfig.INTERVALO_ENVIO_AUTOMATICO_MS)
        }
    }

    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(20.dp)
        ) {
            Text(
                text = "Pulsera Inteligente",
                style = MaterialTheme.typography.headlineMedium
            )

            Spacer(modifier = Modifier.height(16.dp))
            Text(
                text = "Servidor: ${NetworkingConfig.BASE_URL}",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Estado actual: $estado")
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Último evento: $ultimoEvento")
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Frecuencia cardíaca: $heartRateText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Pasos (24h): $pasosText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Estrés: $estresText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Sueño: $suenoText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Actividad: $actividadText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Calorías: $caloriasText")
            Spacer(modifier = Modifier.height(6.dp))
            Text(text = "Alerta actual: $alertaActual")

            Spacer(modifier = Modifier.height(20.dp))

            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                Button(
                    onClick = {
                        estado = "CRITICO"
                        ultimoEvento = "Botón SOS activado"
                        alertaActual = "Emergencia manual"
                        historial.add(0, "${horaActual()} - SOS activado / estado crítico")

                        scope.launch {
                            val (ok, mensaje) = enviarSosAlBackend()
                            historial.add(
                                0,
                                "${horaActual()} - ${if (ok) "✓" else "✗"} $mensaje"
                            )
                        }
                    }
                ) {
                    Text("SOS")
                }

                Button(
                    onClick = {
                        estado = "NORMAL"
                        ultimoEvento = "Estado restablecido"
                        alertaActual = "Sin alertas"
                        historial.add(0, "${horaActual()} - Estado restablecido")
                    }
                ) {
                    Text("Restablecer")
                }
            }

            Spacer(modifier = Modifier.height(12.dp))

            when (sdkStatus) {
                HealthConnectClient.SDK_AVAILABLE -> {
                    Text(
                        text = "Envío automático cada ${NetworkingConfig.INTERVALO_ENVIO_AUTOMATICO_MINUTOS} min (app abierta o en segundo plano). Sync Samsung Health → Health Connect.",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.height(8.dp))
                    Button(
                        onClick = {
                            permissionLauncher.launch(permissions)
                        }
                    ) {
                        Text("Permisos Health Connect")
                    }
                    Spacer(modifier = Modifier.height(8.dp))
                    Button(
                        onClick = {
                            scope.launch {
                                val client = clientProvider()
                                val granted = client.permissionController.getGrantedPermissions()
                                ejecutarSincronizacionUi(
                                    context = appContext,
                                    client = client,
                                    permisosOk = permissions.all { it in granted },
                                    forzarEnvio = true,
                                    manual = true,
                                    horaActual = ::horaActual,
                                    historial = historial,
                                    onHeartRateText = { heartRateText = it },
                                    onPasosText = { pasosText = it },
                                    onEstresText = { estresText = it },
                                    onSuenoText = { suenoText = it },
                                    onActividadText = { actividadText = it },
                                    onCaloriasText = { caloriasText = it },
                                    onEstado = { estado = it },
                                    onAlerta = { alertaActual = it },
                                    onUltimoEvento = { ultimoEvento = it },
                                )
                            }
                        }
                    ) {
                        Text("Enviar ahora (manual)")
                    }
                }

                HealthConnectClient.SDK_UNAVAILABLE_PROVIDER_UPDATE_REQUIRED -> {
                    Button(onClick = onOpenHealthConnect) {
                        Text("Instalar o actualizar Health Connect")
                    }
                }

                else -> {
                    Text("Health Connect no está disponible en este dispositivo")
                }
            }

            Spacer(modifier = Modifier.height(24.dp))

            Text(
                text = "Historial local",
                style = MaterialTheme.typography.titleMedium
            )

            Spacer(modifier = Modifier.height(8.dp))

            LazyColumn(modifier = Modifier.fillMaxWidth()) {
                items(historial) { item ->
                    Text(text = item)
                    Spacer(modifier = Modifier.height(6.dp))
                }
            }
        }
    }
}