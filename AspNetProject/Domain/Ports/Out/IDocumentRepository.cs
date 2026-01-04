using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Repositorio específico para la entidad Document.
/// </summary>
public interface IDocumentRepository : IRepository<Document, Guid>
{
    /// <summary>
    /// Obtiene documentos por tenant.
    /// </summary>
    Task<IEnumerable<Document>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
