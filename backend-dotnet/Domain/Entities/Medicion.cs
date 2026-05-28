using Domain.Enums;

namespace Domain.Entities;

public class Medicion
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int ValorMedicion { get; set; }
    public int? PasosActividad { get; set; }
    public int? NivelEstres { get; set; }
    public int? MinutosSueno { get; set; }
    public int? MinutosActividad { get; set; }
    public int? CaloriasQuemadas { get; set; }
    public EstadoClinico Estado { get; set; }
    public string? MensajeAlerta { get; set; }
    public string OrigenDato { get; set; } = string.Empty;
    public bool EsFueraDeRango { get; set; }

    public Paciente Paciente { get; set; } = null!;

    public TipoMedicion Tipo { get; set; }

    public void SetEstadoClinico(EstadoClinico estado)
    {
        Estado = estado;
        EsFueraDeRango = estado == EstadoClinico.ADVERTENCIA
                      || estado == EstadoClinico.CRITICO;
    }

    public static Medicion CrearMedicionBase(int pacienteId,
                                             int Valor,
                                             DateTime Fecha,
                                             string? origen)
    {
        return new Medicion
        {
            PacienteId = pacienteId,
            ValorMedicion = Valor,
            FechaHora = Fecha,
            OrigenDato = string.IsNullOrWhiteSpace(origen) ? "Desconocido" : origen.Trim(),
        };
    }
}
