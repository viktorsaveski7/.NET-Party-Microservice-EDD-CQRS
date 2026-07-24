namespace PartyService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class;
} // this interface requires an implementation for the PublishAsync method to publish an event and it does not care how you publish it (RabbitMQ, Kafka)