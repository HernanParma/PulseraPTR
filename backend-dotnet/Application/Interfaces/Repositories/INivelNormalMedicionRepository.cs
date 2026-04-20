using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface INivelNormalMedicionRepository
    {
        Task<IReadOnlyCollection<NivelNormalMedicion>> GetValoresNormalPorTipoMedicion(TipoMedicion tipoMed);
    }
}
