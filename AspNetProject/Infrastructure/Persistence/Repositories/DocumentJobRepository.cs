using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence.Repositories;

public class DocumentJobRepository : EfRepository<DocumentProcessingJob, Guid>, IDocumentJobRepository
{
    public DocumentJobRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<DocumentProcessingJob?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(j => j.DocumentId == documentId, cancellationToken);
    }

    public async Task<DocumentProcessingJob?> GetNextQueuedJobAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(j => j.Status == AspNetProject.Domain.ValueObjects.ProcessingStatus.QUEUED)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
