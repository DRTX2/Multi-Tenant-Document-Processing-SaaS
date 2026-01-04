using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence.Repositories;

public class DocumentRepository : EfRepository<Document, Guid>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Document>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
