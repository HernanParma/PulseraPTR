package com.pulseraptr.network

/**
 * Resultado de una llamada al backend (para historial local y UI).
 */
sealed class SyncOutcome {
    data class Success(val lineaHistorial: String) : SyncOutcome()
    data class Failure(val kind: FailureKind, val lineaHistorial: String) : SyncOutcome()
}

enum class FailureKind {
    /** Sin red, DNS, conexión rechazada, host inexistente */
    BACKEND_DOWN,

    /** Base URL mal formada o ilegal */
    INVALID_URL,

    /** Connect / read timeout */
    TIMEOUT,

    /** HTTP 4xx/5xx */
    HTTP_ERROR,

    UNKNOWN
}
