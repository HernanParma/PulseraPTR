package com.example.reloj

import android.os.Bundle
import android.widget.TextView
import androidx.activity.ComponentActivity

class PrivacyPolicyActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val textView = TextView(this).apply {
            text =
                "Esta app usa datos de salud del usuario únicamente para mostrar métricas, " +
                        "detectar alertas y construir un historial clínico dentro del proyecto académico."
            textSize = 18f
            setPadding(40, 40, 40, 40)
        }

        setContentView(textView)
    }
}