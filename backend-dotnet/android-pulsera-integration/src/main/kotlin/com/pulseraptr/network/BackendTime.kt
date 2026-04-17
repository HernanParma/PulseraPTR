package com.pulseraptr.network

import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

/** Formato compatible con el backend .NET para `fechaHora` (sin offset). */
object BackendTime {

    private val formatter: ThreadLocal<SimpleDateFormat> = ThreadLocal.withInitial {
        SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US).apply {
            timeZone = TimeZone.getDefault()
        }
    }

    fun nowIsoLocal(): String = formatter.get().format(Date())
}
