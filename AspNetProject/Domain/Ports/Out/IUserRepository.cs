using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Repositorio específico para la entidad TenantUser.
/// </summary>
public interface IUserRepository : IRepository<TenantUser, Guid>
{
    /// <summary>
    /// Busca un usuario por email dentro de un tenant.
    /// </summary>
    Task<TenantUser?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene usuarios por tenant.
    /// </summary>
    Task<IEnumerable<TenantUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
