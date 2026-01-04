using System.Data;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Abstracción para el manejo de transacciones unitarias.
/// Permite agrupar múltiples operaciones de repositorios en una sola transacción atómica.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Guarda todos los cambios pendientes en el contexto de persistencia.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros afectados</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
