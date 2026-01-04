using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence.Repositories;

public class AuditRepository : EfRepository<AuditLog, Guid>, IAuditRepository
{
    public AuditRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetWithFiltersAsync(Guid? tenantId, Guid? userId, DateTime? startDate, DateTime? endDate, string? resource,
        string? action, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(a => a.TenantId == tenantId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);
        
        if (startDate.HasValue)
            query = query.Where(a => a.OccurredAt >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(a => a.OccurredAt <= endDate.Value);
        
        if (!string.IsNullOrEmpty(resource))
            query = query.Where(a => a.Resource == resource);
        
        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        return await query
            .OrderByDescending(a => a.OccurredAt)
            .Take(1000) // Safety cap
            .ToListAsync(cancellationToken);
    }
}
