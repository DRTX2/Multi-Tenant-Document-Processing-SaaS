using AspNetProject.Domain.Events;

namespace AspNetProject.Domain.Models;

public abstract class AggregateRoot<TId> : IEntity<TId>
{
    public TId Id { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
