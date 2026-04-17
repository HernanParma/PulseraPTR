package com.pulseraptr.network.dto

import com.google.gson.annotations.SerializedName

/**
 * Payload POST /api/eventos/sos
 */
data class EventoSosRequest(
    @SerializedName("pacienteId")
    val pacienteId: Long,

    @SerializedName("fechaHora")
    val fechaHora: String,

    @SerializedName("tipoEvento")
    val tipoEvento: TipoEventoEmergencia = TipoEventoEmergencia.SOS,

    @SerializedName("estado")
    val estado: EstadoClinico = EstadoClinico.CRITICO,

    @SerializedName("mensaje")
    val mensaje: String
)
