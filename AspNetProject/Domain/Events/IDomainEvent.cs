namespace AspNetProject.Domain.Events;

/// <summary>
/// Marcador para eventos de dominio. 
/// Encapsula algo que pasó en el dominio que es de interés para otros componentes.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
