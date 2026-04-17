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
import androidx.compose.ui.unit.dp
import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.PermissionController
import androidx.health.connect.client.permission.HealthPermission
import androidx.health.connect.client.records.HeartRateRecord
import androidx.health.connect.client.request.ReadRecordsRequest
import androidx.health.connect.client.time.TimeRangeFilter
import com.pulseraptr.network.NetworkingConfig
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject
import java.text.SimpleDateFormat
import java.time.Instant
import java.time.temporal.ChronoUnit
import java.util.Date
import java.util.Locale

private const val HEALTH_CONNECT_PACKAGE = "com.google.android.apps.healthdata"

private val httpClient by lazy { OkHttpClient() }

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

fun clasificarEstadoFrecuenciaCardiaca(bpm: Long): String {
    return when {
        bpm >= 120 -> "CRITICO"
        bpm > 100 -> "ADVERTENCIA"
        bpm < 50 -> "ADVERTENCIA"
        else -> "NORMAL"
    }
}

fun generarMensajeAlerta(bpm: Long): String {
    return when {
        bpm >= 120 -> "Frecuencia cardíaca críticamente alta"
        bpm > 100 -> "Frecuencia cardíaca alta"
        bpm < 50 -> "Frecuencia cardíaca baja"
        else -> "Frecuencia cardíaca normal"
    }
}

suspend fun enviarMedicionAlBackend(
    frecuenciaCardiaca: Long,
    estado: String,
    mensajeAlerta: String
): Pair<Boolean, String> = withContext(Dispatchers.IO) {
    try {
        val url = "${NetworkingConfig.BASE_URL}api/mediciones"

        val json = JSONObject().apply {
            put("pacienteId", NetworkingConfig.DEFAULT_PACIENTE_ID)
            put("fechaHora", Instant.now().toString())
            put("frecuenciaCardiaca", frecuenciaCardiaca)
            put("estado", estado)
            put("mensajeAlerta", mensajeAlerta)
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

@Composable
fun PantallaPrincipal(
    sdkStatus: Int,
    clientProvider: () -> HealthConnectClient,
    onOpenHealthConnect: () -> Unit
) {
    val scope = rememberCoroutineScope()

    var estado by remember { mutableStateOf("NORMAL") }
    var ultimoEvento by remember { mutableStateOf("Sin eventos") }
    var heartRateText by remember { mutableStateOf("Sin lectura todavía") }
    var alertaActual by remember { mutableStateOf("Sin alertas") }

    val historial = remember { mutableStateListOf<String>() }

    fun horaActual(): String {
        return SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())
    }

    val permissions = setOf(
        HealthPermission.getReadPermission(HeartRateRecord::class)
    )

    val permissionLauncher =
        rememberLauncherForActivityResult(
            PermissionController.createRequestPermissionResultContract()
        ) { granted ->
            if (permissions.all { it in granted }) {
                ultimoEvento = "Permiso de frecuencia cardíaca otorgado"
                historial.add(0, "${horaActual()} - Permiso Health Connect otorgado")
            } else {
                ultimoEvento = "Permiso denegado"
                historial.add(0, "${horaActual()} - Permiso Health Connect denegado")
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
            Text(text = "Estado actual: $estado")
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Último evento: $ultimoEvento")
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Frecuencia cardíaca: $heartRateText")
            Spacer(modifier = Modifier.height(8.dp))
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
                    Button(
                        onClick = {
                            permissionLauncher.launch(permissions)
                        }
                    ) {
                        Text("Conectar Health Connect")
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    Button(
                        onClick = {
                            scope.launch {
                                try {
                                    val client = clientProvider()

                                    val granted = client.permissionController.getGrantedPermissions()

                                    if (!granted.containsAll(permissions)) {
                                        heartRateText = "No se puede leer FC"
                                        ultimoEvento = "Falta permiso de frecuencia cardíaca"
                                        historial.add(
                                            0,
                                            "${horaActual()} - Falló lectura: permiso no otorgado"
                                        )
                                        return@launch
                                    }

                                    val response = client.readRecords(
                                        ReadRecordsRequest(
                                            recordType = HeartRateRecord::class,
                                            timeRangeFilter = TimeRangeFilter.between(
                                                Instant.now().minus(30, ChronoUnit.DAYS),
                                                Instant.now()
                                            )
                                        )
                                    )

                                    val records = response.records

                                    if (records.isEmpty()) {
                                        heartRateText = "Sin datos de FC"
                                        ultimoEvento = "Health Connect no devolvió registros"
                                        historial.add(
                                            0,
                                            "${horaActual()} - Falló lectura: no hay registros de FC"
                                        )
                                        return@launch
                                    }

                                    val ultimo = records.last()
                                    val sample = ultimo.samples.lastOrNull()

                                    if (sample == null) {
                                        heartRateText = "Registro vacío"
                                        ultimoEvento = "El registro existe pero no tiene muestras"
                                        historial.add(
                                            0,
                                            "${horaActual()} - Falló lectura: registro sin muestras"
                                        )
                                        return@launch
                                    }

                                    val bpm = sample.beatsPerMinute.toLong()
                                    val nuevoEstado = clasificarEstadoFrecuenciaCardiaca(bpm)
                                    val mensajeAlerta = generarMensajeAlerta(bpm)

                                    heartRateText = "$bpm bpm"
                                    estado = nuevoEstado
                                    alertaActual = mensajeAlerta
                                    ultimoEvento = "Lectura de FC obtenida"

                                    historial.add(
                                        0,
                                        "${horaActual()} - FC: $bpm bpm / Estado: $nuevoEstado / $mensajeAlerta"
                                    )

                                    val (ok, mensaje) = enviarMedicionAlBackend(
                                        frecuenciaCardiaca = bpm,
                                        estado = nuevoEstado,
                                        mensajeAlerta = mensajeAlerta
                                    )

                                    historial.add(
                                        0,
                                        "${horaActual()} - ${if (ok) "✓" else "✗"} $mensaje"
                                    )

                                } catch (e: Exception) {
                                    heartRateText = "Error al leer Health Connect"
                                    ultimoEvento = e.message ?: "Error desconocido"
                                    historial.add(
                                        0,
                                        "${horaActual()} - Error leyendo Health Connect"
                                    )
                                }
                            }
                        }
                    ) {
                        Text("Leer frecuencia cardíaca")
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