namespace AspNetProject.Domain.Models;

public class AuditLog : IEntity<Guid>
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }

    public string Action { get; private set; }
    public string Resource { get; private set; }

    public string IpAddress { get; private set; }

    public DateTime OccurredAt { get; private set; }

    private AuditLog() 
    {
        Action = null!;
        Resource = null!;
        IpAddress = null!;
    } // For EF Core

    public AuditLog(
        Guid tenantId,
        string action,
        string resource,
        string ipAddress,
        Guid? userId = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        OccurredAt = DateTime.UtcNow;
    }
}