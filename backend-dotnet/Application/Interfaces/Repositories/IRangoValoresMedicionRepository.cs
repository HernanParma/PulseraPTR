using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IRangoValoresMedicionRepository
    {
        Task<IReadOnlyCollection<RangoValoresMedicion>> GetRangoSegunTipoMedicion(TipoMedicion tipoMed);
        Task<RangoValoresMedicion> Insert(RangoValoresMedicion rango);

        Task<IReadOnlyCollection<RangoValoresMedicion>> GetAll();
    }
}
