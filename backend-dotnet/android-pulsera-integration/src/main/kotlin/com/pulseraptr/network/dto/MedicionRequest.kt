package com.pulseraptr.network.dto

import com.google.gson.annotations.SerializedName

/**
 * Payload POST /api/mediciones
 * (coincide con CrearMedicionDto del backend; estado en JSON en mayúsculas).
 */
data class MedicionRequest(
    @SerializedName("pacienteId")
    val pacienteId: Long,

    /** ISO-8601 local, ej: "2026-04-16T21:10:00" */
    @SerializedName("fechaHora")
    val fechaHora: String,

    @SerializedName("frecuenciaCardiaca")
    val frecuenciaCardiaca: Int,

    @SerializedName("estado")
    val estado: EstadoClinico,

    @SerializedName("mensajeAlerta")
    val mensajeAlerta: String,

    @SerializedName("origenDato")
    val origenDato: String = "HealthConnect"
)
