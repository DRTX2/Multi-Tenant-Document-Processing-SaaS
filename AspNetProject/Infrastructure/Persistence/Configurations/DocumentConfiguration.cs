using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetProject.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.TenantId).IsRequired();
        builder.Property(d => d.OwnerUserId).IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Value Object: DocumentMetadata
        builder.OwnsOne(d => d.Metadata, meta =>
        {
            meta.Property(m => m.FileName).HasColumnName("Meta_FileName").HasMaxLength(255).IsRequired();
            meta.Property(m => m.ContentType).HasColumnName("Meta_ContentType").HasMaxLength(100);
            meta.Property(m => m.SizeInBytes).HasColumnName("Meta_Size");
        });

        // Value Object Collection: Versions
        // Use backing field name because property is read-only IReadOnlyCollection
        builder.OwnsMany<DocumentVersion>("_versions", v =>
        {
            v.ToTable("DocumentVersions");
            v.WithOwner().HasForeignKey("DocumentId");
            v.Property<int>("Id"); // Shadow PK
            v.HasKey("Id");
            
            v.Property(x => x.VersionNumber).IsRequired();
            v.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
            v.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            v.Property(x => x.CreatedAt);
        });
        // Iterate: The builder.OwnsMany(expression) expects a Property or Field access expression.
        // d.GetVersions() is a Method Call. This is NOT allowed in this context usually.
        // We must use the field name string since it's private.
    }
}
