using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetProject.Infrastructure.Persistence.Configurations;

public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // EF Core 8 primitive collection support
        // This will map to a JSON column or Array depending on provider. 
        // For Postgres with Npgsql, it creates a text[] or integer[] array usually.
        builder.PrimitiveCollection(u => u.Roles)
            .HasColumnName("Roles");
            
        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        
        builder.Ignore(u => u.DomainEvents);
    }
}
