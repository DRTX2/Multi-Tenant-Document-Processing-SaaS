using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Repositorio específico para logs de auditoría.
/// </summary>
public interface IAuditRepository : IRepository<AuditLog, Guid>
{
    /// <summary>
    /// Obtiene logs con filtros.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetWithFiltersAsync(
        Guid? tenantId, 
        Guid? userId, 
        DateTime? startDate, 
        DateTime? endDate, 
        string? resource,
        string? action,
        CancellationToken cancellationToken = default);
}
