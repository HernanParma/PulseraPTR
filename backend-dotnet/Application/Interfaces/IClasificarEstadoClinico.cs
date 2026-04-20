using Domain.Entities;

namespace Application.Interfaces
{
    internal interface IClasificarEstadoClinico
    {
        Task ClasificarEstado(Medicion medicion);
    }
}
