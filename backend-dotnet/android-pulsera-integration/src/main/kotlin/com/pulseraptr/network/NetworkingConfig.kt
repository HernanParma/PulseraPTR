package com.pulseraptr.network

/**
 * **CAMBIAR LA IP AQUÍ** (único sitio obligatorio para apuntar al PC).
 *
 * Tu backend .NET en la PC NO es "localhost" para el teléfono: usá la IP LAN del PC.
 * Ejemplo: si tu PC es 192.168.1.100 y Kestrel escucha HTTP en 5093:
 *
 *   const val BASE_URL = "http://192.168.1.100:5093/"
 *
 * Importante:
 * - Debe terminar en "/" (Retrofit).
 * - Misma Wi‑Fi que el teléfono; firewall de Windows debe permitir conexiones entrantes al puerto.
 * - Si usás perfil "http" del launchSettings, el puerto suele ser **5093** (no 5000).
 */
object NetworkingConfig {

    const val BASE_URL: String = "http://192.168.1.100:5093/"

    /** Id del paciente que existe en SQL Server (creado desde dashboard o POST /api/pacientes). */
    const val DEFAULT_PACIENTE_ID: Long = 1L

    /** Logs OkHttp en Logcat (etiqueta OkHttp). Desactivar en release si copiás este archivo tal cual. */
    const val ENABLE_HTTP_LOGGING: Boolean = true
}
