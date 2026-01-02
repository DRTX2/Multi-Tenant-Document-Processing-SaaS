using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Models;

public class Tenant : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    public TenantConfiguration Configuration { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tenant() 
    {
        Name = null!;
        Configuration = null!;
    } // For EF Core

    public Tenant(string name, TenantConfiguration configuration)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Status = TenantStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = TenantStatus.SUSPENDED;
    }

    public void Activate()
    {
        Status = TenantStatus.ACTIVE;
    }

    public void Delete()
    {
        Status = TenantStatus.DELETED;
    }

    public void UpdateConfiguration(TenantConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
    }
}