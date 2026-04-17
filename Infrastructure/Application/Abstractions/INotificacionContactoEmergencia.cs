namespace Application.Abstractions;

/// <summary>
/// Aviso al contacto de emergencia del paciente (SMS, email, push, etc.). La implementación concreta vive en Infrastructure o en la capa de presentación.
/// </summary>
public interface INotificacionContactoEmergencia
{
    Task EnviarAvisoAsync(
        int pacienteId,
        string nombrePaciente,
        string contactoEmergencia,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default);
}
