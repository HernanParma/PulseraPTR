package com.pulseraptr.network

import com.pulseraptr.network.dto.EstadoClinico

/**
 * Misma lógica que en el backend / app local (FC y SOS en UI).
 * Reutilizá esto para no duplicar if/else en MainActivity.
 */
object ClasificacionFc {

    fun clasificar(lpm: Int): EstadoClinico {
        if (lpm >= 120) return EstadoClinico.CRITICO
        if (lpm > 100) return EstadoClinico.ADVERTENCIA
        if (lpm < 50) return EstadoClinico.ADVERTENCIA
        return EstadoClinico.NORMAL
    }

    fun mensajeAlerta(estado: EstadoClinico, lpm: Int): String = when (estado) {
        EstadoClinico.NORMAL -> "Frecuencia cardíaca normal ($lpm lpm)"
        EstadoClinico.ADVERTENCIA -> "Frecuencia cardíaca fuera del rango óptimo ($lpm lpm)"
        EstadoClinico.CRITICO -> "Frecuencia cardíaca crítica ($lpm lpm)"
    }
}
