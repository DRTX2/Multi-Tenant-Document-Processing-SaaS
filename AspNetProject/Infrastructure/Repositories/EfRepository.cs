using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Infrastructure.Data;

namespace AspNetProject.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del repositorio usando Entity Framework Core
/// Esta clase base evita duplicación de código entre repositorios
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad</typeparam>
/// <typeparam name="TId">Tipo del identificador</typeparam>
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId> 
    where TEntity : class, IEntity<TId>
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    #region Consultas

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    #endregion

    #region Comandos

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities, 
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entityList;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<TEntity> entities, 
        CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        await Context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
