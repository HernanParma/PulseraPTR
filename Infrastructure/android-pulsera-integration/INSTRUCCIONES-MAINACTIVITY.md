# Integración en tu `MainActivity`

Copiá la carpeta Kotlin `com/pulseraptr/network/` dentro de `app/src/main/java/` (o `kotlin/`) de tu proyecto Android, **manteniendo el paquete** `com.pulseraptr.network` **o** renombrá el paquete y actualizá los `import`.

## 1) Propiedades en la Activity

Dentro de `MainActivity` (o el `Activity` donde tengas HR + SOS):

```kotlin
import androidx.lifecycle.lifecycleScope
import com.pulseraptr.network.NetworkingConfig
import com.pulseraptr.network.PulseraSyncCoordinator

private lateinit var pulseraSync: PulseraSyncCoordinator

override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    // ... tu setContentView / bindings ...

    pulseraSync = PulseraSyncCoordinator(
        onHistorialLine = { line -> runOnUiThread { agregarLineaHistorial(line) } },
        scope = lifecycleScope
    )
}
```

Sustituí `agregarLineaHistorial` por **tu función real** que escribe en el historial (TextView, RecyclerView, Room, etc.).

## 2) Cuando leas FC desde Health Connect

**Después** de mostrar el valor en pantalla, clasificar y guardar historial local, agregá:

```kotlin
import com.pulseraptr.network.ClasificacionFc

val estado = ClasificacionFc.clasificar(bpm)
val mensaje = ClasificacionFc.mensajeAlerta(estado, bpm)

// ... tu historial local existente ...

pulseraSync.enviarMedicionAsync(
    pacienteId = NetworkingConfig.DEFAULT_PACIENTE_ID,
    frecuenciaCardiaca = bpm,
    estado = estado,
    mensajeAlerta = mensaje
)
```

El POST es **asíncrono**: no bloquea la UI; si falla, verás la línea en el historial con prefijo `✗` y el motivo.

## 3) Botón SOS

Después de poner estado local CRITICO + historial local:

```kotlin
pulseraSync.enviarSosAsync(
    pacienteId = NetworkingConfig.DEFAULT_PACIENTE_ID,
    mensaje = "Emergencia manual"
)
```

## 4) Dónde cambiar la IP del PC

**Archivo:** `NetworkingConfig.kt`  
**Constante:** `BASE_URL` (debe terminar en `/`).

También podés cambiar `DEFAULT_PACIENTE_ID` al id real del paciente en tu base.

## 5) Permiso INTERNET

En `AndroidManifest.xml`, dentro de `<manifest>`:

```xml
<uses-permission android:name="android.permission.INTERNET" />
```
