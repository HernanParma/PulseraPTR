using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>
    /// clasificador de estado para mediciones , recibe una medicion y le asigna
    /// un estado clinico
    /// </summary>
    public interface IClasificarEstadoClinico
    {
        Task ClasificarMedicion(Medicion medicion);
    }
}
