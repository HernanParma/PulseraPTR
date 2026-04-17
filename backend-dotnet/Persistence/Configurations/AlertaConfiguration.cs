using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("Alertas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FechaHora).IsRequired();
        builder.Property(a => a.Mensaje).HasMaxLength(1000).IsRequired();
        builder.HasIndex(a => new { a.PacienteId, a.FechaHora });

        builder.HasOne(a => a.Paciente)
            .WithMany(p => p.Alertas)
            .HasForeignKey(a => a.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
