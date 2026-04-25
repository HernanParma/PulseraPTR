using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Workers;

/// <summary>
/// Sondea IMAP (Gmail), importa adjuntos CSV mySugr con <see cref="IGlucoseImportService"/> y mueve mensajes a etiquetas.
/// </summary>
public sealed class GlucoseEmailImportWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<GlucoseEmailImportOptions> _optionsMonitor;
    private readonly ILogger<GlucoseEmailImportWorker> _logger;

    public GlucoseEmailImportWorker(
        IServiceProvider serviceProvider,
        IOptionsMonitor<GlucoseEmailImportOptions> optionsMonitor,
        ILogger<GlucoseEmailImportWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[GlucoseEmail] Worker registrado: se ejecutará en segundo plano (IMAP cada N minutos según GlucoseEmailImport:CheckIntervalMinutes).");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            var delay = TimeSpan.FromMinutes(Math.Max(1, options.CheckIntervalMinutes));

            try
            {
                if (!options.Enabled)
                {
                    _logger.LogInformation(
                        "[GlucoseEmail] Importación por correo desactivada (GlucoseEmailImport:Enabled=false). Próximo chequeo en {Min} min.",
                        delay.TotalMinutes);
                }
                else if (string.IsNullOrWhiteSpace(options.Password))
                {
                    _logger.LogWarning(
                        "[GlucoseEmail] Enabled=true pero no hay contraseña en configuración. Usá user-secrets: GlucoseEmailImport:Password o env GlucoseEmailImport__Password. Próximo intento en {Min} min.",
                        delay.TotalMinutes);
                }
                else
                {
                    await PollInboxOnceAsync(options, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en ciclo GlucoseEmailImport; el worker sigue activo.");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("[GlucoseEmail] Worker detenido.");
    }

    private async Task PollInboxOnceAsync(GlucoseEmailImportOptions options, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();

        try
        {
            var socketOptions = options.UseSsl
                ? MailKit.Security.SecureSocketOptions.SslOnConnect
                : MailKit.Security.SecureSocketOptions.StartTls;

            _logger.LogInformation(
                "[GlucoseEmail] Conectando a {Host}:{Port} (SSL implícito={UseSsl})…",
                options.Host,
                options.Port,
                options.UseSsl);

            await client.ConnectAsync(options.Host, options.Port, socketOptions, cancellationToken);
            _logger.LogInformation("[GlucoseEmail] Conexión IMAP establecida (TLS).");

            await client.AuthenticateAsync(options.Username, options.Password!, cancellationToken);
            _logger.LogInformation("[GlucoseEmail] Autenticación IMAP correcta (usuario {User}).", options.Username);

            if (client.Capabilities.HasFlag(ImapCapabilities.GMailExt1))
                _logger.LogInformation("[GlucoseEmail] Servidor Gmail (X-GM-EXT1): búsqueda X-GM-RAW disponible.");
            else
                _logger.LogWarning("[GlucoseEmail] Sin extensión Gmail X-GM-EXT1; la búsqueda por asunto usa SUBJECT IMAP (menos fiable en Gmail).");

            var inbox = client.GetFolder(options.InboxFolderName);
            await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
            _logger.LogInformation("[GlucoseEmail] Carpeta \"{Folder}\" abierta (ReadWrite).", options.InboxFolderName);

            IMailFolder? processedFolder = TryGetFolder(client, options.ProcessedFolderName, "procesados");
            IMailFolder? errorFolder = TryGetFolder(client, options.ErrorFolderName, "errores");

            if (processedFolder is null && options.MarkAsSeenOnSuccessIfMoveFails)
            {
                _logger.LogWarning(
                    "[GlucoseEmail] Etiqueta/carpeta \"{Processed}\" no existe en IMAP. Tras importar OK se marcará leído (MarkAsSeenOnSuccessIfMoveFails).",
                    options.ProcessedFolderName);
            }

            if (errorFolder is null)
            {
                _logger.LogWarning(
                    "[GlucoseEmail] Etiqueta/carpeta \"{Error}\" no existe. Errores: mover fallará; revisá MarkAsSeenOnErrorIfMoveFails.",
                    options.ErrorFolderName);
            }

            var (searchQuery, searchDescription) = BuildInboxSearchQuery(client, options);
            _logger.LogInformation("[GlucoseEmail] Búsqueda: {Query}", searchDescription);

            var uids = await inbox.SearchAsync(searchQuery, cancellationToken);
            _logger.LogInformation("[GlucoseEmail] Mensajes coincidentes: {Count}.", uids.Count);

            var filter = options.ImapSubjectFilter?.Trim();
            if (uids.Count == 0
                && options.PreferGmailRawSearch
                && client.Capabilities.HasFlag(ImapCapabilities.GMailExt1)
                && !string.IsNullOrEmpty(filter))
            {
                _logger.LogWarning(
                    "[GlucoseEmail] X-GM-RAW devolvió 0 resultados; reintentando con UNSEEN + SUBJECT (fallback IMAP estándar).");
                searchQuery = SearchQuery.And(SearchQuery.NotSeen, SearchQuery.SubjectContains(filter));
                uids = await inbox.SearchAsync(searchQuery, cancellationToken);
                _logger.LogInformation("[GlucoseEmail] Tras fallback SUBJECT: {Count} mensaje(s).", uids.Count);
            }

            if (uids.Count == 0)
            {
                _logger.LogInformation("[GlucoseEmail] Nada que procesar en este ciclo.");
                return;
            }

            foreach (var uid in uids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessSingleMessageAsync(inbox, uid, processedFolder, errorFolder, options, cancellationToken);
            }

            _logger.LogInformation("[GlucoseEmail] Ciclo terminado: procesados {Count} mensaje(s).", uids.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GlucoseEmail] Error en la sesión IMAP ({Host}). Revisá red, IMAP habilitado en Gmail y contraseña de aplicación.", options.Host);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(true, cancellationToken);
                    _logger.LogInformation("[GlucoseEmail] Desconectado del servidor IMAP.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[GlucoseEmail] Aviso al cerrar sesión IMAP (ignorado).");
                }
            }
        }
    }

    /// <summary>
    /// Gmail a menudo no hace coincidir SEARCH SUBJECT con lo que ves en la web; X-GM-RAW replica la búsqueda web.
    /// </summary>
    private static (SearchQuery Query, string Description) BuildInboxSearchQuery(
        ImapClient client,
        GlucoseEmailImportOptions options)
    {
        var filter = options.ImapSubjectFilter?.Trim();
        if (string.IsNullOrEmpty(filter))
            return (SearchQuery.NotSeen, "NotSeen (sin filtro de asunto)");

        if (options.PreferGmailRawSearch && client.Capabilities.HasFlag(ImapCapabilities.GMailExt1))
        {
            var raw = BuildGmailRawExpression(filter, options);
            return (SearchQuery.GMailRawSearch(raw), $"Gmail X-GM-RAW: {raw}");
        }

        var q = SearchQuery.And(SearchQuery.NotSeen, SearchQuery.SubjectContains(filter));
        return (q, $"NotSeen AND SUBJECT contains \"{filter}\"");
    }

    private static string BuildGmailRawExpression(string filter, GlucoseEmailImportOptions options)
    {
        var term = filter.Contains(' ', StringComparison.Ordinal)
                   || filter.Contains(':', StringComparison.Ordinal)
                   || filter.Contains('"', StringComparison.Ordinal)
            ? "\"" + filter.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\""
            : filter;

        var extra = options.GmailRawRequireAttachment ? " has:attachment" : string.Empty;
        return $"in:inbox is:unread subject:{term}{extra}";
    }

    private IMailFolder? TryGetFolder(ImapClient client, string path, string logical)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            return client.GetFolder(path);
        }
        catch (FolderNotFoundException ex)
        {
            _logger.LogWarning(ex, "[GlucoseEmail] Carpeta IMAP \"{Path}\" ({Logical}) no existe. Creala como etiqueta en Gmail.", path, logical);
            return null;
        }
    }

    private async Task ProcessSingleMessageAsync(
        IMailFolder inbox,
        UniqueId uid,
        IMailFolder? processedFolder,
        IMailFolder? errorFolder,
        GlucoseEmailImportOptions options,
        CancellationToken cancellationToken)
    {
        MimeMessage message;
        try
        {
            message = await inbox.GetMessageAsync(uid, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GlucoseEmail] No se pudo descargar el mensaje UID {Uid}.", uid);
            await MoveOrFlagErrorAsync(inbox, uid, errorFolder, options, cancellationToken, "fallo al descargar mensaje");
            return;
        }

        var subject = message.Subject ?? string.Empty;
        _logger.LogInformation("[GlucoseEmail] Procesando UID={Uid} | Asunto: {Subject}", uid, subject);

        if (!GlucoseEmailSubjectParser.TryExtractPacienteId(subject, out var pacienteId))
        {
            _logger.LogWarning(
                "[GlucoseEmail] UID={Uid}: no se encontró \"PacienteId: N\" en el asunto. Asunto actual: \"{Subject}\". → error / carpeta error.",
                uid, subject);
            await MoveOrFlagErrorAsync(inbox, uid, errorFolder, options, cancellationToken, "sin PacienteId en asunto");
            return;
        }

        _logger.LogInformation("[GlucoseEmail] UID={Uid}: PacienteId detectado = {PacienteId}.", uid, pacienteId);

        var part = FindFirstCsvAttachment(message);
        if (part is null)
        {
            _logger.LogWarning(
                "[GlucoseEmail] UID={Uid}: no hay adjunto .csv (asunto: \"{Subject}\"). → error.",
                uid, subject);
            await MoveOrFlagErrorAsync(inbox, uid, errorFolder, options, cancellationToken, "sin adjunto CSV");
            return;
        }

        var fileName = part.FileName ?? part.ContentType?.Name ?? "mysugr.csv";
        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            fileName += ".csv";

        try
        {
            if (part.Content is null)
            {
                _logger.LogWarning("[GlucoseEmail] UID={Uid}: adjunto CSV sin cuerpo decodificable.", uid);
                await MoveOrFlagErrorAsync(inbox, uid, errorFolder, options, cancellationToken, "CSV sin contenido");
                return;
            }

            _logger.LogInformation("[GlucoseEmail] UID={Uid}: adjunto CSV encontrado → \"{FileName}\".", uid, fileName);

            await using var stream = new MemoryStream();
            await part.Content.DecodeToAsync(stream, cancellationToken);
            stream.Position = 0;

            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var import = scope.ServiceProvider.GetRequiredService<IGlucoseImportService>();
                var result = await import.ImportMySugrCsvAsync(pacienteId, stream, fileName, cancellationToken);

                _logger.LogInformation(
                    "[GlucoseEmail] Importación CSV finalizada | PacienteId={PacienteId} | archivo={File} | filasLeídas={Read} | importadas={Imported} | duplicadas={Dup} | descartadas={Disc} | líneasDeError={ErrCount}",
                    pacienteId,
                    fileName,
                    result.RowsRead,
                    result.Imported,
                    result.Duplicates,
                    result.Discarded,
                    result.Errors.Count);

                if (result.Errors.Count > 0)
                {
                    foreach (var err in result.Errors.Take(15))
                        _logger.LogWarning("[GlucoseEmail] Detalle fila/parser: {Error}", err);
                    if (result.Errors.Count > 15)
                        _logger.LogWarning("[GlucoseEmail] … y {More} error(es) más en el resultado.", result.Errors.Count - 15);
                }
            }

            await MoveOrFlagSuccessAsync(inbox, uid, processedFolder, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GlucoseEmail] Excepción al importar CSV (PacienteId={PacienteId}, UID={Uid}).", pacienteId, uid);
            await MoveOrFlagErrorAsync(inbox, uid, errorFolder, options, cancellationToken, ex.Message);
        }
    }

    private static MimePart? FindFirstCsvAttachment(MimeMessage message)
    {
        foreach (var attachment in message.Attachments)
        {
            if (attachment is not MimePart part)
                continue;

            var name = part.FileName ?? part.ContentType?.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return part;
        }

        return null;
    }

    private async Task MoveOrFlagSuccessAsync(
        IMailFolder inbox,
        UniqueId uid,
        IMailFolder? processedFolder,
        GlucoseEmailImportOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (processedFolder is not null)
            {
                await inbox.MoveToAsync(uid, processedFolder, cancellationToken);
                _logger.LogInformation(
                    "[GlucoseEmail] UID={Uid}: movido a carpeta procesados \"{Folder}\".",
                    uid,
                    options.ProcessedFolderName);
                return;
            }

            if (options.MarkAsSeenOnSuccessIfMoveFails)
            {
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                _logger.LogInformation(
                    "[GlucoseEmail] UID={Uid}: sin carpeta procesados; mensaje marcado como leído (no reintenta).",
                    uid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GlucoseEmail] No se pudo mover/marcar UID={Uid} tras importación exitosa.", uid);
            if (options.MarkAsSeenOnSuccessIfMoveFails)
            {
                try
                {
                    await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                    _logger.LogInformation("[GlucoseEmail] UID={Uid}: marcado como leído tras error al mover.", uid);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "[GlucoseEmail] No se pudo marcar como leído UID={Uid}.", uid);
                }
            }
        }
    }

    private async Task MoveOrFlagErrorAsync(
        IMailFolder inbox,
        UniqueId uid,
        IMailFolder? errorFolder,
        GlucoseEmailImportOptions options,
        CancellationToken cancellationToken,
        string reason)
    {
        try
        {
            if (errorFolder is not null)
            {
                await inbox.MoveToAsync(uid, errorFolder, cancellationToken);
                _logger.LogWarning(
                    "[GlucoseEmail] UID={Uid}: movido a carpeta error \"{Folder}\". Motivo: {Reason}",
                    uid,
                    options.ErrorFolderName,
                    reason);
                return;
            }

            if (options.MarkAsSeenOnErrorIfMoveFails)
            {
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                _logger.LogWarning(
                    "[GlucoseEmail] UID={Uid}: sin carpeta error; marcado como leído. Motivo: {Reason}",
                    uid,
                    reason);
            }
            else
            {
                _logger.LogWarning(
                    "[GlucoseEmail] UID={Uid}: no se movió ni se marcó leído (revisá carpeta error y MarkAsSeenOnErrorIfMoveFails). Motivo: {Reason}",
                    uid,
                    reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GlucoseEmail] Fallo al mover/marcar mensaje erróneo UID={Uid}. Motivo previo: {Reason}", uid, reason);
        }
    }
}
