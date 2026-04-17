🩺 PulseraPTR – Monitoreo de Salud en Tiempo Real
📌 Descripción

PulseraPTR es un sistema integral de monitoreo de salud en tiempo real orientado al seguimiento de pacientes, especialmente adultos mayores. El sistema combina una aplicación móvil Android con un backend desarrollado en .NET, permitiendo capturar, procesar y visualizar datos fisiológicos en tiempo real.

El objetivo principal es detectar situaciones de riesgo de manera temprana, como alteraciones en la frecuencia cardíaca, inactividad o emergencias, facilitando la intervención rápida por parte de familiares o profesionales de la salud.

🏗️ Arquitectura del sistema

El sistema está compuesto por dos grandes módulos:

📱 Aplicación Android (APK)
Desarrollada en Kotlin
Utiliza Health Connect para obtener datos biométricos
Permite:
Lectura de frecuencia cardíaca
Generación de eventos SOS
Envío de datos al backend en tiempo real
Visualización de estado y alertas locales
🌐 Backend .NET + Dashboard Web
Desarrollado con .NET (Clean Architecture)
Base de datos con Entity Framework (Code First)
API REST para recepción de datos
Dashboard web para visualización
🔄 Flujo de funcionamiento
La aplicación Android obtiene datos del usuario (ej: frecuencia cardíaca)
Se analiza el estado del paciente (NORMAL, ADVERTENCIA, CRÍTICO)
Se envían los datos al backend mediante API REST
El backend almacena la información en la base de datos
El dashboard muestra:
Mediciones
Alertas
Eventos SOS
Estado actual del paciente
❤️ Funcionalidades principales
📈 Monitoreo de frecuencia cardíaca en tiempo real
🚨 Generación de alertas automáticas según valores fuera de rango
🆘 Botón SOS para emergencias manuales
🧠 Clasificación del estado clínico:
NORMAL
ADVERTENCIA
CRÍTICO
📊 Dashboard web con historial de mediciones
🔄 Integración en tiempo real entre app y backend
🧪 Ejemplo de lógica implementada
Frecuencia cardíaca:

120 bpm → CRÍTICO

100–120 bpm → ADVERTENCIA
< 50 bpm → ADVERTENCIA
Valores normales → NORMAL
🧑‍⚕️ Enfoque clínico

El sistema está orientado al monitoreo remoto de pacientes, permitiendo:

Detección temprana de eventos cardiovasculares
Seguimiento continuo sin necesidad de consulta presencial
Supervisión de pacientes vulnerables (adultos mayores, pacientes crónicos)
Registro histórico para toma de decisiones médicas
🛠️ Tecnologías utilizadas
Backend
.NET
Entity Framework Core
SQL Server
API REST
Frontend Web
HTML / CSS / JavaScript
Aplicación móvil
Kotlin
Android Studio
Health Connect API
OkHttp (comunicación HTTP)
📂 Estructura del proyecto
PulseraPTR/
│
├── backend-dotnet/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   └── PulseraPTR (API + Web)
│
├── android-app/
│   └── Reloj (App Android)
│
└── README.md
🚀 Ejecución del proyecto
Backend
Abrir la solución en Visual Studio / Rider
Ejecutar el proyecto API
Acceder a:
Swagger: http://localhost:5093/swagger
Dashboard: https://localhost:7052/
Android
Abrir el proyecto en Android Studio
Conectar dispositivo físico
Ejecutar la app
Asegurar permisos de Health Connect
📡 Comunicación en red

La app Android se conecta al backend mediante la IP local del equipo:

http://<IP_LOCAL>:5093/

Es necesario que:

PC y celular estén en la misma red WiFi
El backend esté en ejecución
El firewall permita conexiones entrantes
🎯 Objetivo académico

Este proyecto fue desarrollado como trabajo práctico integrador en la materia:

Programación en Tiempo Real – Ingeniería en Informática

Se aplicaron conceptos como:

Sistemas en tiempo real
Comunicación cliente-servidor
Procesamiento de eventos
Arquitectura en capas
Integración de hardware/software
🔮 Posibles mejoras futuras
Integración con más sensores (SpO₂, actividad física)
Detección automática de caídas
Monitoreo de inactividad prolongada
Notificaciones en tiempo real (push)
Multiusuario y autenticación
Integración con servicios médicos
