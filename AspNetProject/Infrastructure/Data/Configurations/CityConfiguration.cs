using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetProject.Domain.Models;

namespace AspNetProject.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad City
/// Separa la configuración de la entidad para mantener el dominio limpio
/// </summary>
public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        // Configuración de tabla
        builder.ToTable("cities");

        // Clave primaria
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        // Propiedades
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Latitude)
            .IsRequired()
            .HasPrecision(9, 6); // Precisión para coordenadas geográficas

        builder.Property(c => c.Longitude)
            .IsRequired()
            .HasPrecision(9, 6);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        // Índices
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("ix_cities_name");

        builder.HasIndex(c => c.Country)
            .HasDatabaseName("ix_cities_country");

        builder.HasIndex(c => new { c.Latitude, c.Longitude })
            .HasDatabaseName("ix_cities_coordinates");
    }
}
