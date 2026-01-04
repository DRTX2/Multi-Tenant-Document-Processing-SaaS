using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace AspNetProject.Infrastructure.Persistence.Repositories;

public class TenantRepository : EfRepository<Tenant, Guid>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}
