using Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

/// <summary>
/// Implementación de desarrollo: registra el aviso en logs. Sustituí por SMS (Twilio), email (SMTP), etc. cuando tengas credenciales.
/// </summary>
public class LoggingNotificacionContactoEmergencia : INotificacionContactoEmergencia
{
    private readonly ILogger<LoggingNotificacionContactoEmergencia> _logger;

    public LoggingNotificacionContactoEmergencia(ILogger<LoggingNotificacionContactoEmergencia> logger)
    {
        _logger = logger;
    }

    public Task EnviarAvisoAsync(
        int pacienteId,
        string nombrePaciente,
        string contactoEmergencia,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[Contacto emergencia] PacienteId={PacienteId} Paciente={Nombre} Contacto={Contacto} | {Titulo} | {Mensaje}",
            pacienteId,
            nombrePaciente,
            contactoEmergencia,
            titulo,
            mensaje);

        return Task.CompletedTask;
    }
}
