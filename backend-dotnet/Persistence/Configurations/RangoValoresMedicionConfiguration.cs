using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RangoValoresMedicionConfiguration : IEntityTypeConfiguration<RangoValoresMedicion>
{
    public void Configure(EntityTypeBuilder<RangoValoresMedicion> builder)
    {
        builder.ToTable("RangoValoresMedicion");
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.RangoEdadMinimo)
            .IsRequired();

        builder.Property(r => r.RangoEdadMaximo)
            .IsRequired();

        builder.Property(r => r.ValorNormalMinimo)
            .IsRequired();

        builder.Property(r => r.ValorNormalMaximo)
            .IsRequired();

        builder.Property(r => r.ValorCriticoMinimo)
            .IsRequired();

        builder.Property(r => r.ValorCriticoMaximo)
            .IsRequired();

        //restricciones de valores
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_RangoEdadMinimo",
                "[RangoEdadMinimo] <> 0");

            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_RangoEdadMaximo",
                "[RangoEdadMaximo] <> 0");

            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_ValorNormalMinimo",
                "[ValorNormalMinimo] <> 0");

            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_ValorNormalMaximo",
                "[ValorNormalMaximo] <> 0");

            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_ValorCriticoMinimo",
                "[ValorCriticoMinimo] <> 0");

            t.HasCheckConstraint(
                "CK_RangoValoresMedicion_ValorCriticoMaximo",
                "[ValorCriticoMaximo] <> 0");
        });
    }
}
