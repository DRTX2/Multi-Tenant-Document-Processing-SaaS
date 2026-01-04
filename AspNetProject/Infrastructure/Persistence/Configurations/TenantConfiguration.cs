using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetProject.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Value Object: TenantConfiguration
        builder.OwnsOne(t => t.Configuration, config =>
        {
            config.Property(c => c.MaxStorageMb).HasColumnName("Config_MaxStorageMb");
            config.Property(c => c.OcrEnabled).HasColumnName("Config_OcrEnabled");
            config.Property(c => c.RateLimitPerMinute).HasColumnName("Config_RateLimit");
        });

        builder.Ignore(t => t.DomainEvents);
    }
}
