package com.pulseraptr.network

import android.util.Log
import com.pulseraptr.network.api.ApiService
import com.pulseraptr.network.dto.EventoSosRequest
import com.pulseraptr.network.dto.MedicionRequest
import retrofit2.HttpException
import java.io.InterruptedIOException
import java.net.ConnectException
import java.net.MalformedURLException
import java.net.SocketTimeoutException
import java.net.UnknownHostException
import javax.net.ssl.SSLException

/**
 * Capa reutilizable: toda la lógica HTTP vive acá (no repetir en Activities).
 */
class PulseraRemoteRepository(
    private val api: ApiService = RetrofitClient.api
) {

    suspend fun enviarMedicion(request: MedicionRequest): SyncOutcome {
        Log.d(TAG, "POST medición pacienteId=${request.pacienteId} fc=${request.frecuenciaCardiaca}")
        return execute("Medición") {
            val response = api.postMedicion(request)
            if (response.isSuccessful) {
                SyncOutcome.Success(
                    "[${BackendTime.nowIsoLocal()}] ✓ Medición enviada al servidor (${request.frecuenciaCardiaca} lpm, ${request.estado})"
                )
            } else {
                val err = response.errorBody()?.string().orEmpty().ifBlank { response.message() }
                SyncOutcome.Failure(
                    FailureKind.HTTP_ERROR,
                    "[${BackendTime.nowIsoLocal()}] ✗ Medición rechazada HTTP ${response.code()}: $err"
                )
            }
        }
    }

    suspend fun enviarSos(request: EventoSosRequest): SyncOutcome {
        Log.d(TAG, "POST SOS pacienteId=${request.pacienteId}")
        return execute("SOS") {
            val response = api.postEventoSos(request)
            if (response.isSuccessful) {
                SyncOutcome.Success(
                    "[${BackendTime.nowIsoLocal()}] ✓ SOS enviado al servidor"
                )
            } else {
                val err = response.errorBody()?.string().orEmpty().ifBlank { response.message() }
                SyncOutcome.Failure(
                    FailureKind.HTTP_ERROR,
                    "[${BackendTime.nowIsoLocal()}] ✗ SOS rechazado HTTP ${response.code()}: $err"
                )
            }
        }
    }

    private inline fun execute(label: String, block: () -> SyncOutcome): SyncOutcome {
        return try {
            block()
        } catch (e: HttpException) {
            val body = e.response()?.errorBody()?.string().orEmpty()
            Log.e(TAG, "$label HttpException ${e.code()}", e)
            SyncOutcome.Failure(
                FailureKind.HTTP_ERROR,
                "[${BackendTime.nowIsoLocal()}] ✗ $label HTTP ${e.code()}: ${body.ifBlank { e.message() }}"
            )
        } catch (e: UnknownHostException) {
            Log.e(TAG, "$label UnknownHost", e)
            SyncOutcome.Failure(
                FailureKind.BACKEND_DOWN,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: no se alcanza el servidor (host/DNS). ¿IP o Wi‑Fi?"
            )
        } catch (e: ConnectException) {
            Log.e(TAG, "$label ConnectException", e)
            SyncOutcome.Failure(
                FailureKind.BACKEND_DOWN,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: conexión rechazada. ¿Backend encendido y puerto abierto?"
            )
        } catch (e: SocketTimeoutException) {
            Log.e(TAG, "$label timeout", e)
            SyncOutcome.Failure(
                FailureKind.TIMEOUT,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: tiempo de espera agotado (timeout)"
            )
        } catch (e: InterruptedIOException) {
            Log.e(TAG, "$label interrupted IO", e)
            SyncOutcome.Failure(
                FailureKind.TIMEOUT,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: timeout o conexión interrumpida"
            )
        } catch (e: MalformedURLException) {
            Log.e(TAG, "$label URL inválida", e)
            SyncOutcome.Failure(
                FailureKind.INVALID_URL,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: URL inválida (${NetworkingConfig.BASE_URL})"
            )
        } catch (e: IllegalArgumentException) {
            // Retrofit también usa esto si la baseUrl es inválida al crear el cliente
            Log.e(TAG, "$label IllegalArgument", e)
            SyncOutcome.Failure(
                FailureKind.INVALID_URL,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: configuración de URL inválida"
            )
        } catch (e: SSLException) {
            Log.e(TAG, "$label SSL", e)
            SyncOutcome.Failure(
                FailureKind.HTTP_ERROR,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: error SSL/certificado (¿https sin confiar cert?)"
            )
        } catch (e: Exception) {
            Log.e(TAG, "$label error inesperado", e)
            SyncOutcome.Failure(
                FailureKind.UNKNOWN,
                "[${BackendTime.nowIsoLocal()}] ✗ $label: ${e.javaClass.simpleName} — ${e.message}"
            )
        }
    }

    companion object {
        private const val TAG = "PulseraRemote"
    }
}
