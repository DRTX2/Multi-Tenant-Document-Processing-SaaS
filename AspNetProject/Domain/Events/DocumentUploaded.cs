namespace AspNetProject.Domain.Events;

public record DocumentUploaded(Guid DocumentId, Guid TenantId, string Filename) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
