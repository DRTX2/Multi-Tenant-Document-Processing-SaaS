using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Repositorio específico para la entidad Tenant.
/// </summary>
public interface ITenantRepository : IRepository<Tenant, Guid>
{
    /// <summary>
    /// Busca un tenant por su nombre (único).
    /// </summary>
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
