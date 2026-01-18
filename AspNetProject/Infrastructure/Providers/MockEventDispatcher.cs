using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AspNetProject.Infrastructure.Providers;

public class MockEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<MockEventDispatcher> _logger;
    
    public MockEventDispatcher(ILogger<MockEventDispatcher> logger) => _logger = logger;

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PUBLISHING EVENT: {EventType} occurred at {Date}", domainEvent.GetType().Name, domainEvent.OccurredOn);
        return Task.CompletedTask;
    }
}

