using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA genérico (Outbound Port) para operaciones CRUD simples.
/// Este puerto es implementado por la capa de Infrastructure (EfRepository)
/// y usado por la capa de Application.
/// 
/// PRINCIPIO: El repositorio solo maneja persistencia básica.
/// Para consultas complejas, usa Query Services específicos del dominio.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad del dominio</typeparam>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    // ============================================
    // CONSULTAS BÁSICAS (Read)
    // ============================================
    
    /// <summary>
    /// Obtiene una entidad por su identificador.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe una entidad con el ID especificado.
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cuenta el total de entidades.
    /// NOTA: Para obtener listas de entidades, usa Query Services con paginación.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    
    // ============================================
    // COMANDOS (Create, Update, Delete)
    // ============================================
    
    /// <summary>
    /// Agrega una nueva entidad.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agrega múltiples entidades en una sola operación.
    /// 
    /// ⚠️ NOTA: La implementación debe garantizar atomicidad.
    /// Si falla una entidad, todas deben revertirse.
    /// </summary>
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina una entidad por su ID.
    /// </summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina una entidad.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina múltiples entidades en una sola operación.
    /// 
    /// ⚠️ NOTA: La implementación debe garantizar atomicidad.
    /// Si falla una eliminación, todas deben revertirse.
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
