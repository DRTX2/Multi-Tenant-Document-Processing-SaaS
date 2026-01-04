using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence.Repositories;

public class UserRepository : EfRepository<TenantUser, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<TenantUser?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);
    }

    public async Task<IEnumerable<TenantUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
