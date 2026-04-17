using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Dni).HasMaxLength(32);
        builder.Property(p => p.ContactoEmergencia).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Observaciones).HasMaxLength(2000);
        builder.HasIndex(p => p.Dni).IsUnique().HasFilter("[Dni] IS NOT NULL");
    }
}
