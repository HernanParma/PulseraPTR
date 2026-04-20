using Application.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mapping;

public static class PulseraMapper
{
    public static PacienteDto ToDto(this Paciente p, EstadoClinico? estadoActual = null, DateTime? ultimaMedicion = null) =>
        new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Edad = p.Edad,
            Dni = p.Dni,
            ContactoEmergencia = p.ContactoEmergencia,
            Observaciones = p.Observaciones,
            Activo = p.Activo,
            EstadoActual = estadoActual,
            UltimaMedicionFechaHora = ultimaMedicion
        };

    public static MedicionDto ToDto(this Medicion m) =>
        new()
        {
            Id = m.Id,
            PacienteId = m.PacienteId,
            PacienteNombre = m.Paciente?.Nombre,
            FechaHora = m.FechaHora,
            FrecuenciaCardiaca = m.ValorMedicion,
            Estado = m.Estado,
            MensajeAlerta = m.MensajeAlerta,
            OrigenDato = m.OrigenDato,
            EsFueraDeRango = m.EsFueraDeRango
        };

    public static AlertaDto ToDto(this Alerta a) =>
        new()
        {
            Id = a.Id,
            PacienteId = a.PacienteId,
            PacienteNombre = a.Paciente?.Nombre,
            FechaHora = a.FechaHora,
            TipoAlerta = a.TipoAlerta,
            Estado = a.Estado,
            Mensaje = a.Mensaje,
            Leida = a.Leida
        };

    public static EventoEmergenciaDto ToDto(this EventoEmergencia e) =>
        new()
        {
            Id = e.Id,
            PacienteId = e.PacienteId,
            PacienteNombre = e.Paciente?.Nombre,
            FechaHora = e.FechaHora,
            TipoEvento = e.TipoEvento,
            Estado = e.Estado,
            Mensaje = e.Mensaje,
            Atendido = e.Atendido
        };
}
