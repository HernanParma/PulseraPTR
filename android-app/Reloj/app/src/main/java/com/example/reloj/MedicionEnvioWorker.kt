package com.example.reloj

import android.content.Context
import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.permission.HealthPermission
import androidx.health.connect.client.records.ActiveCaloriesBurnedRecord
import androidx.health.connect.client.records.ExerciseSessionRecord
import androidx.health.connect.client.records.HeartRateRecord
import androidx.health.connect.client.records.HeartRateVariabilityRmssdRecord
import androidx.health.connect.client.records.SleepSessionRecord
import androidx.health.connect.client.records.StepsRecord
import androidx.health.connect.client.records.TotalCaloriesBurnedRecord
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import com.pulseraptr.network.NetworkingConfig

class MedicionEnvioWorker(
    appContext: Context,
    params: WorkerParameters,
) : CoroutineWorker(appContext, params) {

    override suspend fun doWork(): Result {
        val status = HealthConnectClient.getSdkStatus(
            applicationContext,
            HEALTH_CONNECT_PACKAGE
        )
        if (status != HealthConnectClient.SDK_AVAILABLE) {
            MedicionEnvioScheduler.programarSiguiente(applicationContext)
            return Result.success()
        }

        val client = HealthConnectClient.getOrCreate(applicationContext)
        val granted = client.permissionController.getGrantedPermissions()
        val permisosOk = healthPermissions.all { it in granted }

        val resultado = MedicionSyncHelper.sincronizar(
            context = applicationContext,
            client = client,
            permisosConcedidos = permisosOk,
            forzarEnvio = false,
        )

        MedicionEnvioScheduler.programarSiguiente(applicationContext)
        return if (resultado.error != null && !resultado.omitidoPorIntervalo) {
            Result.retry()
        } else {
            Result.success()
        }
    }

    companion object {
        const val WORK_NAME = "pulsera_medicion_envio"
        private const val HEALTH_CONNECT_PACKAGE = "com.google.android.apps.healthdata"

        val healthPermissions = setOf(
            HealthPermission.getReadPermission(HeartRateRecord::class),
            HealthPermission.getReadPermission(StepsRecord::class),
            HealthPermission.getReadPermission(SleepSessionRecord::class),
            HealthPermission.getReadPermission(ExerciseSessionRecord::class),
            HealthPermission.getReadPermission(ActiveCaloriesBurnedRecord::class),
            HealthPermission.getReadPermission(TotalCaloriesBurnedRecord::class),
            HealthPermission.getReadPermission(HeartRateVariabilityRmssdRecord::class),
        )
    }
}

object MedicionEnvioScheduler {

    fun iniciarCadena(context: Context) {
        val work = androidx.work.OneTimeWorkRequestBuilder<MedicionEnvioWorker>()
            .addTag(MedicionEnvioWorker.WORK_NAME)
            .build()
        androidx.work.WorkManager.getInstance(context).enqueueUniqueWork(
            MedicionEnvioWorker.WORK_NAME,
            androidx.work.ExistingWorkPolicy.REPLACE,
            work,
        )
    }

    fun programarSiguiente(context: Context) {
        val work = androidx.work.OneTimeWorkRequestBuilder<MedicionEnvioWorker>()
            .setInitialDelay(
                NetworkingConfig.INTERVALO_ENVIO_AUTOMATICO_MINUTOS,
                java.util.concurrent.TimeUnit.MINUTES,
            )
            .addTag(MedicionEnvioWorker.WORK_NAME)
            .build()
        androidx.work.WorkManager.getInstance(context).enqueue(work)
    }
}
