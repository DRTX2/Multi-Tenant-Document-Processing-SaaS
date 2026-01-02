using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Models;

public class TenantUser : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    
    public string Email { get; private set; }
    public UserStatus Status { get; private set; }
    
    private readonly List<UserRole> _roles;
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }

    private TenantUser() 
    {
        Email = null!;
        _roles = new List<UserRole>();
    } // For EF Core

    public TenantUser(Guid tenantId, string email, params UserRole[] roles)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Status = UserStatus.ACTIVE;
        _roles = roles.Length > 0 ? roles.ToList() : new List<UserRole> { UserRole.USER };
        CreatedAt = DateTime.UtcNow;
    }

    public void Lock()
    {
        Status = UserStatus.LOCKED;
    }

    public void Unlock()
    {
        Status = UserStatus.ACTIVE;
    }

    public void Disable()
    {
        Status = UserStatus.DISABLED;
    }

    public void AddRole(UserRole role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(UserRole role)
    {
        _roles.Remove(role);
    }
}