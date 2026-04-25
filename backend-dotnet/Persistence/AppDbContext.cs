using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medicion> Mediciones => Set<Medicion>();
    public DbSet<EventoEmergencia> EventosEmergencia => Set<EventoEmergencia>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<GlucoseReading> GlucoseReadings => Set<GlucoseReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Paciente técnico fijo (Id=22) para integración Android / tiempo real.
        // Se aplica vía migración (HasData); no se duplica al re-ejecutar la app, solo al aplicar migraciones pendientes.
        modelBuilder.Entity<Paciente>().HasData(
            new Paciente
            {
                Id = 22,
                Nombre = "Reloj en tiempo real",
                Edad = 1,
                Dni = null,
                ContactoEmergencia = "N/A (paciente técnico)",
                Observaciones = "Paciente técnico para integración con app Android en tiempo real",
                Activo = true
            });

        base.OnModelCreating(modelBuilder);
    }
}
