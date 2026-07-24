// Added because of the Outbox Pattern
namespace PartyService.Application.Interfaces;

public interface IOutboxEventPublisher
{
    Task PublishToOutboxAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class;
}