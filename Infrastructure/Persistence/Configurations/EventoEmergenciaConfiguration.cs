using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EventoEmergenciaConfiguration : IEntityTypeConfiguration<EventoEmergencia>
{
    public void Configure(EntityTypeBuilder<EventoEmergencia> builder)
    {
        builder.ToTable("EventosEmergencia");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FechaHora).IsRequired();
        builder.Property(e => e.Mensaje).HasMaxLength(1000).IsRequired();
        builder.HasIndex(e => new { e.PacienteId, e.FechaHora });

        builder.HasOne(e => e.Paciente)
            .WithMany(p => p.EventosEmergencia)
            .HasForeignKey(e => e.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
