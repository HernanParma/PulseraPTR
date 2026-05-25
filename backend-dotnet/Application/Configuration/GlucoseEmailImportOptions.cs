namespace Application.Configuration;

/// <summary>
/// IMAP (p. ej. Gmail) para importación automática de CSV mySugr.
/// La contraseña NO debe estar en appsettings: usar user-secrets o variable de entorno <c>GlucoseEmailImport__Password</c>.
/// </summary>
public sealed class GlucoseEmailImportOptions
{
    public const string SectionName = "GlucoseEmailImport";

    /// <summary>Desactivado por defecto hasta configurar credenciales y etiquetas en Gmail.</summary>
    public bool Enabled { get; set; }

    public string Host { get; set; } = "imap.gmail.com";
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;

    /// <summary>Cuenta IMAP (ej. hernanparma22@gmail.com).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Contraseña de aplicación de Google; enlazar solo por secrets/env.</summary>
    public string? Password { get; set; }

    public int CheckIntervalMinutes { get; set; } = 10;

    public string InboxFolderName { get; set; } = "INBOX";

    /// <summary>
    /// Texto que debe aparecer en el asunto para que el servidor IMAP devuelva el mensaje (SEARCH SUBJECT).
    /// Evita procesar miles de promos no leídas: usar <c>PacienteId</c> (mismo criterio que el parser del asunto).
    /// Vacío = solo NotSeen (no recomendado en Gmail con mucho correo).
    /// </summary>
    public string ImapSubjectFilter { get; set; } = "PacienteId";

    /// <summary>
    /// En Gmail, usar X-GM-RAW (misma sintaxis que la barra de búsqueda web). Evita que SUBJECT IMAP devuelva 0 con correos visibles como no leídos.
    /// </summary>
    public bool PreferGmailRawSearch { get; set; } = true;

    /// <summary>
    /// Si es true, la expresión X-GM-RAW incluye <c>has:attachment</c> (más estricto; desactivar si Gmail no indexa el adjunto).
    /// </summary>
    public bool GmailRawRequireAttachment { get; set; }

    /// <summary>
    /// Nombre de etiqueta/carpeta IMAP en Gmail (crear en la web). Ej: <c>GlucoseImport/Procesados</c>.
    /// </summary>
    public string ProcessedFolderName { get; set; } = "GlucoseImport/Procesados";

    public string ErrorFolderName { get; set; } = "GlucoseImport/Errores";

    /// <summary>
    /// Si no existe la carpeta de procesados, marcar el mensaje como leído para no reprocesarlo en bucle.
    /// </summary>
    public bool MarkAsSeenOnSuccessIfMoveFails { get; set; } = true;

    /// <summary>
    /// Si falla el movimiento a carpeta de error, marcar como leído para evitar reintentos infinitos (revisar logs).
    /// </summary>
    public bool MarkAsSeenOnErrorIfMoveFails { get; set; } = true;
}
