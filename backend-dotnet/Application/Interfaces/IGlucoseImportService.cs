using Application.Dtos.Glucose;

namespace Application.Interfaces;

public interface IGlucoseImportService
{
    Task<GlucoseImportResultDto> ImportMySugrCsvAsync(
        int pacienteId,
        Stream csvStream,
        string fileName,
        GlucoseImportMetadata? metadata = null,
        CancellationToken cancellationToken = default);
}
