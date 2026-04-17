package com.pulseraptr.network

import com.pulseraptr.network.api.ApiService
import com.google.gson.GsonBuilder
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

/**
 * Instancia única de Retrofit + [ApiService].
 * La base URL se toma de [NetworkingConfig.BASE_URL].
 */
object RetrofitClient {

    private val gson by lazy {
        GsonBuilder()
            .serializeNulls()
            .create()
    }

    private val okHttpClient: OkHttpClient by lazy {
        val b = OkHttpClient.Builder()
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
        if (NetworkingConfig.ENABLE_HTTP_LOGGING) {
            b.addInterceptor(
                HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BODY }
            )
        }
        b.build()
    }

    val retrofit: Retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(NetworkingConfig.BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create(gson))
            .build()
    }

    val api: ApiService by lazy { retrofit.create(ApiService::class.java) }
}
