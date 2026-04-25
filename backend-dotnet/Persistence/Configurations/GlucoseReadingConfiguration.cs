using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GlucoseReadingConfiguration : IEntityTypeConfiguration<GlucoseReading>
{
    public void Configure(EntityTypeBuilder<GlucoseReading> builder)
    {
        builder.ToTable("GlucoseReadings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReadingDateTime).IsRequired();
        builder.Property(x => x.DateRaw).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TimeRaw).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GlucoseMgDl).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceFileName).HasMaxLength(260);
        builder.Property(x => x.Source).HasConversion<int>().IsRequired();
        builder.Property(x => x.ImportHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.PacienteId, x.ImportHash }).IsUnique();
        builder.HasIndex(x => new { x.PacienteId, x.ReadingDateTime });

        builder.HasOne(x => x.Paciente)
            .WithMany(p => p.GlucoseReadings)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
