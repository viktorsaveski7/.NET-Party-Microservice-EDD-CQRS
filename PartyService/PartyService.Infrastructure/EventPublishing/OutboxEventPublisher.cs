// Added because of the Outbox Pattern
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;
using System.Text.Json;

namespace PartyService.Infrastructure.EventPublishing;

public class OutboxEventPublisher : IOutboxEventPublisher
{
    private readonly IOutboxRepository _outboxRepository;

    public OutboxEventPublisher(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task PublishToOutboxAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        // Serialize event to JSON
        var eventData = JsonSerializer.Serialize(@event);

        // Create outbox message
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(TEvent).Name,
            EventData = eventData,
            RoutingKey = routingKey,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false,
            RetryCount = 0
        };

        // Save to outbox table
        await _outboxRepository.AddAsync(outboxMessage);
    }
}