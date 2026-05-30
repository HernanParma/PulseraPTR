package com.pulseraptr.network

/**
 * Configuración de red para conectar la APK con el backend .NET
 *
 * IMPORTANTE:
 * - Usar la IP local de tu PC (ipconfig → Wi-Fi), no localhost
 * - Debe terminar en "/"
 * - Celular y PC en la misma WiFi (misma red, ej. ambos 192.168.0.x)
 */

object NetworkingConfig {

    // IP LAN actual de la PC (misma subred que el celular: 192.168.0.x)
    const val BASE_URL: String = "http://192.168.0.198:5093/"

    const val DEFAULT_PACIENTE_ID: Long = 22L

    const val INTERVALO_ENVIO_AUTOMATICO_MINUTOS: Long = 10L

    val INTERVALO_ENVIO_AUTOMATICO_MS: Long
        get() = INTERVALO_ENVIO_AUTOMATICO_MINUTOS * 60_000L

    const val ENABLE_HTTP_LOGGING: Boolean = true
}
