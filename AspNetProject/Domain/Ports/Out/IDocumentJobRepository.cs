using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Repositorio para gestión de trabajos de procesamiento.
/// </summary>
public interface IDocumentJobRepository : IRepository<DocumentProcessingJob, Guid>
{
    Task<DocumentProcessingJob?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<DocumentProcessingJob?> GetNextQueuedJobAsync(CancellationToken cancellationToken = default);
}
