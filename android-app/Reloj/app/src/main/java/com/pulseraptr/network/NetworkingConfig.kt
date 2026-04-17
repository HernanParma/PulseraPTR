package com.pulseraptr.network

/**
 * Configuración de red para conectar la APK con el backend .NET
 *
 * IMPORTANTE:
 * - Usar la IP local de tu PC (no localhost)
 * - Debe terminar en "/"
 * - El backend debe estar corriendo
 * - El celular y la PC deben estar en la misma WiFi
 */

object NetworkingConfig {

    // IP de tu PC + puerto HTTP del backend .NET
    const val BASE_URL: String = "http://192.168.0.196:5093/"

    // ID del paciente existente en tu base de datos
    const val DEFAULT_PACIENTE_ID: Long = 22L

    // Logs HTTP (útil para debug en Logcat)
    const val ENABLE_HTTP_LOGGING: Boolean = true
}