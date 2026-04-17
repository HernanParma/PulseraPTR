using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MedicionConfiguration : IEntityTypeConfiguration<Medicion>
{
    public void Configure(EntityTypeBuilder<Medicion> builder)
    {
        builder.ToTable("Mediciones");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FechaHora).IsRequired();
        builder.Property(m => m.FrecuenciaCardiaca).IsRequired();
        builder.Property(m => m.MensajeAlerta).HasMaxLength(1000);
        builder.Property(m => m.OrigenDato).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => new { m.PacienteId, m.FechaHora });

        builder.HasOne(m => m.Paciente)
            .WithMany(p => p.Mediciones)
            .HasForeignKey(m => m.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
