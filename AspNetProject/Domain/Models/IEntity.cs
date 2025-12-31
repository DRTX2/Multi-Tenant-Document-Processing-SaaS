namespace AspNetProject.Domain.Models;

/// <summary>
/// Interfaz base para todas las entidades del dominio
/// </summary>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public interface IEntity<TId>
{
    TId Id { get; set; }
}
