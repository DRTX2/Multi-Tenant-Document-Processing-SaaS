using AspNetProject.Domain.Events;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA para publicar eventos de dominio.
/// Implementado por Infrastructure (usando MediatR, RabbitMQ, etc).
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
