package com.pulseraptr.network

import com.pulseraptr.network.dto.EstadoClinico
import com.pulseraptr.network.dto.EventoSosRequest
import com.pulseraptr.network.dto.MedicionRequest
import com.pulseraptr.network.dto.TipoEventoEmergencia
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

/**
 * API cómoda desde la UI: construye DTOs, llama al [PulseraRemoteRepository] y notifica resultado al historial.
 * No bloquea el hilo principal (IO en [Dispatchers.IO]).
 */
class PulseraSyncCoordinator(
    private val repository: PulseraRemoteRepository = PulseraRemoteRepository(),
    private val onHistorialLine: (String) -> Unit,
    private val scope: CoroutineScope
) {

    fun enviarMedicionAsync(
        pacienteId: Long = NetworkingConfig.DEFAULT_PACIENTE_ID,
        frecuenciaCardiaca: Int,
        estado: EstadoClinico,
        mensajeAlerta: String,
        origenDato: String = "HealthConnect"
    ) {
        scope.launch {
            val outcome = withContext(Dispatchers.IO) {
                val request = MedicionRequest(
                    pacienteId = pacienteId,
                    fechaHora = BackendTime.nowIsoLocal(),
                    frecuenciaCardiaca = frecuenciaCardiaca,
                    estado = estado,
                    mensajeAlerta = mensajeAlerta,
                    origenDato = origenDato
                )
                repository.enviarMedicion(request)
            }
            when (outcome) {
                is SyncOutcome.Success -> onHistorialLine(outcome.lineaHistorial)
                is SyncOutcome.Failure -> onHistorialLine(outcome.lineaHistorial)
            }
        }
    }

    fun enviarSosAsync(
        pacienteId: Long = NetworkingConfig.DEFAULT_PACIENTE_ID,
        mensaje: String = "Emergencia manual"
    ) {
        scope.launch {
            val outcome = withContext(Dispatchers.IO) {
                val request = EventoSosRequest(
                    pacienteId = pacienteId,
                    fechaHora = BackendTime.nowIsoLocal(),
                    tipoEvento = TipoEventoEmergencia.SOS,
                    estado = EstadoClinico.CRITICO,
                    mensaje = mensaje
                )
                repository.enviarSos(request)
            }
            when (outcome) {
                is SyncOutcome.Success -> onHistorialLine(outcome.lineaHistorial)
                is SyncOutcome.Failure -> onHistorialLine(outcome.lineaHistorial)
            }
        }
    }
}
