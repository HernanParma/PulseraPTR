using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class DataSeeder
{
    /// <param name="seedDemoData">Si es false, no se insertan pacientes/mediciones de ejemplo (solo datos reales vía API).</param>
    public static async Task SeedAsync(AppDbContext db, ILogger logger, bool seedDemoData, CancellationToken cancellationToken = default)
    {
        await EnsurePacienteTecnico22PerfilAsync(db, logger, cancellationToken);
        await EnsureGlucemiaDemoFechasRecientesAsync(db, logger, cancellationToken);
        await SeedGlucemiaDemoAsync(db, logger, cancellationToken);

        if (!seedDemoData)
        {
            logger.LogInformation("Seed de demostración desactivado (Pulsera:SeedDemoData=false). Los datos llegan por la API desde la APK.");
            return;
        }

        if (await db.Pacientes.AnyAsync(cancellationToken))
        {
            logger.LogInformation("La base ya contiene pacientes; se omite el seed.");
            return;
        }

        logger.LogInformation("Ejecutando seed inicial de PulseraPTR...");

        var baseFecha = new DateTime(2026, 4, 16, 8, 0, 0, DateTimeKind.Unspecified);

        var p1 = new Paciente
        {
            Nombre = "Rosa Martínez",
            Edad = 78,
            Dni = "12-3456789",
            ContactoEmergencia = "María Martínez - 11-2222-3333",
            Observaciones = "Hipertensión controlada",
            Activo = true
        };

        var p2 = new Paciente
        {
            Nombre = "Jorge Pérez",
            Edad = 82,
            Dni = "98-7654321",
            ContactoEmergencia = "Lucas Pérez - 11-4444-5555",
            Observaciones = null,
            Activo = true
        };

        var p3 = new Paciente
        {
            Nombre = "Elena Gómez",
            Edad = 75,
            Dni = null,
            ContactoEmergencia = "Carlos Gómez - 11-6666-7777",
            Observaciones = "Alergia a penicilina",
            Activo = true
        };

        db.Pacientes.AddRange(p1, p2, p3);
        await db.SaveChangesAsync(cancellationToken);

        var mediciones = new List<Medicion>
        {
            new()
            {
                PacienteId = p1.Id,
                FechaHora = baseFecha.AddHours(1),
                ValorMedicion = 72,
                Estado = EstadoClinico.NORMAL,
                MensajeAlerta = "Frecuencia cardíaca normal",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = false
            },
            new()
            {
                PacienteId = p1.Id,
                FechaHora = baseFecha.AddHours(3),
                ValorMedicion = 48,
                Estado = EstadoClinico.ADVERTENCIA,
                MensajeAlerta = "Bradicardia leve",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = true
            },
            new()
            {
                PacienteId = p1.Id,
                FechaHora = baseFecha.AddHours(5),
                ValorMedicion = 125,
                Estado = EstadoClinico.CRITICO,
                MensajeAlerta = "Taquicardia",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = true
            },
            new()
            {
                PacienteId = p2.Id,
                FechaHora = baseFecha.AddMinutes(30),
                ValorMedicion = 88,
                Estado = EstadoClinico.NORMAL,
                MensajeAlerta = "En rango",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = false
            },
            new()
            {
                PacienteId = p2.Id,
                FechaHora = baseFecha.AddHours(2),
                ValorMedicion = 110,
                Estado = EstadoClinico.ADVERTENCIA,
                MensajeAlerta = "FC elevada",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = true
            },
            new()
            {
                PacienteId = p2.Id,
                FechaHora = baseFecha.AddHours(4),
                ValorMedicion = 95,
                Estado = EstadoClinico.NORMAL,
                MensajeAlerta = "Recuperación",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = false
            },
            new()
            {
                PacienteId = p3.Id,
                FechaHora = baseFecha.AddHours(1).AddMinutes(15),
                ValorMedicion = 60,
                Estado = EstadoClinico.NORMAL,
                MensajeAlerta = "Estable",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = false
            },
            new()
            {
                PacienteId = p3.Id,
                FechaHora = baseFecha.AddHours(2),
                ValorMedicion = 122,
                Estado = EstadoClinico.CRITICO,
                MensajeAlerta = "FC crítica",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = true
            },
            new()
            {
                PacienteId = p3.Id,
                FechaHora = baseFecha.AddHours(6),
                ValorMedicion = 76,
                Estado = EstadoClinico.NORMAL,
                MensajeAlerta = "Normal",
                OrigenDato = "HealthConnect",
                EsFueraDeRango = false
            }
        };

        db.Mediciones.AddRange(mediciones);

        var alertas = new List<Alerta>
        {
            new()
            {
                PacienteId = p1.Id,
                FechaHora = baseFecha.AddHours(3),
                TipoAlerta = TipoAlerta.FrecuenciaCardiaca,
                Estado = EstadoClinico.ADVERTENCIA,
                Mensaje = "Bradicardia leve",
                Leida = false
            },
            new()
            {
                PacienteId = p1.Id,
                FechaHora = baseFecha.AddHours(5),
                TipoAlerta = TipoAlerta.FrecuenciaCardiaca,
                Estado = EstadoClinico.CRITICO,
                Mensaje = "Taquicardia",
                Leida = true
            },
            new()
            {
                PacienteId = p2.Id,
                FechaHora = baseFecha.AddHours(2),
                TipoAlerta = TipoAlerta.FrecuenciaCardiaca,
                Estado = EstadoClinico.ADVERTENCIA,
                Mensaje = "FC elevada",
                Leida = false
            },
            new()
            {
                PacienteId = p3.Id,
                FechaHora = baseFecha.AddHours(2),
                TipoAlerta = TipoAlerta.FrecuenciaCardiaca,
                Estado = EstadoClinico.CRITICO,
                Mensaje = "FC crítica",
                Leida = false
            }
        };

        db.Alertas.AddRange(alertas);

        var eventos = new List<EventoEmergencia>
        {
            new()
            {
                PacienteId = p2.Id,
                FechaHora = baseFecha.AddHours(4).AddMinutes(10),
                TipoEvento = TipoEventoEmergencia.SOS,
                Estado = EstadoClinico.CRITICO,
                Mensaje = "Emergencia manual (simulación)",
                Atendido = false
            },
            new()
            {
                PacienteId = p3.Id,
                FechaHora = baseFecha.AddHours(3),
                TipoEvento = TipoEventoEmergencia.SOS,
                Estado = EstadoClinico.CRITICO,
                Mensaje = "SOS de prueba",
                Atendido = true
            }
        };

        db.EventosEmergencia.AddRange(eventos);

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed inicial completado.");
    }

    private static async Task EnsurePacienteTecnico22PerfilAsync(AppDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var paciente = await db.Pacientes.FindAsync(new object[] { 22 }, cancellationToken);
        if (paciente is null)
            return;

        const int edad = 71;
        const string dni = "6.789.112";
        const string contacto = "1123516612 Laura (hija)";

        if (paciente.Edad == edad && paciente.Dni == dni && paciente.ContactoEmergencia == contacto)
            return;

        paciente.Edad = edad;
        paciente.Dni = dni;
        paciente.ContactoEmergencia = contacto;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Paciente 22 (Reloj en tiempo real) actualizado: edad {Edad}, DNI {Dni}.", edad, dni);
    }

    /// <summary>
    /// Si las lecturas demo del paciente 22 quedaron con fechas fuera de la ventana del dashboard, las recorre a los últimos días.
    /// </summary>
    private static async Task EnsureGlucemiaDemoFechasRecientesAsync(AppDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var lecturas = await db.GlucoseReadings
            .Where(r => r.PacienteId == 22 && r.SourceFileName == "seed-demo.csv")
            .OrderBy(r => r.ReadingDateTime)
            .ToListAsync(cancellationToken);

        if (lecturas.Count == 0 || lecturas.Any(r => r.ReadingDateTime >= cutoff))
            return;

        var rng = new Random(42);
        for (var i = 0; i < lecturas.Count; i++)
        {
            var r = lecturas[i];
            var dt = DateTime.UtcNow.AddDays(-(lecturas.Count - 1 - i)).AddHours(8 + rng.Next(10));
            r.ReadingDateTime = dt;
            r.DateRaw = dt.ToString("dd/MM/yyyy");
            r.TimeRaw = dt.ToString("HH:mm");
            r.ImportHash = Application.Services.GlucoseImportHash.Compute(22, dt, r.GlucoseMgDl, r.Label);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Fechas de glucemia demo (paciente 22) actualizadas a los últimos {Count} días.", lecturas.Count);
    }

    private static async Task SeedGlucemiaDemoAsync(AppDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        if (await db.GlucoseReadings.AnyAsync(cancellationToken))
            return;

        if (!await db.Pacientes.AnyAsync(p => p.Id == 22, cancellationToken))
            return;

        logger.LogInformation("Insertando datos ficticios de glucemia para paciente 22...");

        var rng = new Random(42);
        var labels = new[] { "En ayunas", "Después de comer", "Antes de comer" };
        var readings = new List<GlucoseReading>();

        // Fechas relativas a hoy para que entren en la ventana del dashboard (últimos 30 días).
        for (int i = 0; i < 25; i++)
        {
            var dt = DateTime.UtcNow.AddDays(-(24 - i)).AddHours(8 + rng.Next(10));
            int val = i switch
            {
                _ when i % 7 == 0 => rng.Next(55, 85),
                _ when i % 5 == 0 => rng.Next(185, 260),
                _ => rng.Next(75, 155)
            };
            var label = labels[i % 3];
            var hash = Application.Services.GlucoseImportHash.Compute(22, dt, val, label);

            readings.Add(new GlucoseReading
            {
                PacienteId = 22,
                ReadingDateTime = dt,
                DateRaw = dt.ToString("dd/MM/yyyy"),
                TimeRaw = dt.ToString("HH:mm"),
                Label = label,
                GlucoseMgDl = val,
                TimeZone = "GMT-03:00",
                SourceFileName = "seed-demo.csv",
                Source = GlucoseReadingSource.MySugrCsvImport,
                ImportHash = hash,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.GlucoseReadings.AddRangeAsync(readings, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed glucemia: {Count} lecturas insertadas.", readings.Count);
    }
}
