using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public static class GlucoseImportHash
{
    /// <summary>
    /// Hash estable por paciente + instante UTC + valor + etiqueta (deduplicación de reimportaciones).
    /// </summary>
    public static string Compute(int pacienteId, DateTime readingUtc, int glucoseMgDl, string labelNormalized)
    {
        var payload = $"{pacienteId}|{readingUtc:O}|{glucoseMgDl}|{labelNormalized}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
