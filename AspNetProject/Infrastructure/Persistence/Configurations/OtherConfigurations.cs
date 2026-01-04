using AspNetProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetProject.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Resource).HasMaxLength(50).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(45).IsRequired(); // IPv6 length

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.OccurredAt);
    }
}

// public class DocumentProcessingJobConfiguration ...
public class DocumentProcessingJobConfiguration : IEntityTypeConfiguration<DocumentProcessingJob>
{
    public void Configure(EntityTypeBuilder<DocumentProcessingJob> builder)
    {
        builder.ToTable("DocumentProcessingJobs");

        builder.HasKey(j => j.Id);
        
        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(j => j.LastError).HasMaxLength(2000);

        builder.HasIndex(j => j.DocumentId);
        builder.HasIndex(j => j.Status);
    }
}
