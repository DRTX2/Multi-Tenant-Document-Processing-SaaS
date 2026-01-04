using AspNetProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> Users { get; set; } // Table name 'Users' might conflict in some DBs, prefer 'TenantUsers' or schema
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentProcessingJob> DocumentJobs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global Conventions
        
        // Ignore DomainEvents for all AggregateRoots to prevent EF from trying to map them
        modelBuilder.Ignore<AggregateRoot<Guid>>(); // This might not work deeply for inherited props depending on EF version, better to ignore property in config
    }
}
