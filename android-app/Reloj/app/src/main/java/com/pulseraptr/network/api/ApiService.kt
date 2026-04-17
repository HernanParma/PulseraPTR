package com.pulseraptr.network.api

import com.pulseraptr.network.dto.EventoSosRequest
import com.pulseraptr.network.dto.MedicionRequest
import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface ApiService {

    @POST("api/mediciones")
    suspend fun postMedicion(@Body body: MedicionRequest): Response<ResponseBody>

    @POST("api/eventos/sos")
    suspend fun postEventoSos(@Body body: EventoSosRequest): Response<ResponseBody>
}
