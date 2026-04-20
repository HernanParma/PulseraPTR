using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Services
{
    internal class ClasificarEstadoClinicoService : IClasificarEstadoClinico
    {
        private readonly INivelNormalMedicionRepository _niveles;
        private readonly IPacienteRepository _pacientes;

        public ClasificarEstadoClinicoService(INivelNormalMedicionRepository niveles,
                                              IPacienteRepository pacientes)
        {
            _niveles = niveles;
            _pacientes = pacientes;
        }

        public async Task ClasificarEstado(Medicion medicion)
        {
            var nivelesNormales = await _niveles.GetValoresNormalPorTipoMedicion(medicion.Tipo);
            var paciente = await _pacientes.GetByIdAsync(medicion.PacienteId);


            if (paciente is null)
            {
                throw new ArgumentNullException("paciente.no.encontrado");
            }
            int edad = paciente.Edad;

            var nivelNormal = nivelesNormales
                               .Where(nv => nv.RangoEdadMinimo <= edad
                                         && nv.RangoEdadMaximo >= edad)
                                        .FirstOrDefault();

            if (nivelNormal is null)
            {
                throw new ArgumentNullException("nivel.no.existe");
            }

            var estado = nivelNormal.Clasificar(medicion.ValorMedicion);

            medicion.SetEstadoClinico(estado);
        }

    }
}
