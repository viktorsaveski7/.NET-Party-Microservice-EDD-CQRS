using PartyService.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PartyService.Infrastructure.EventPublishing;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;

    public RabbitMqEventPublisher(RabbitMqSettings settings)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _exchangeName = settings.ExchangeName;

        // Declare exchange (creates if doesn't exist)
        _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        ).GetAwaiter().GetResult();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        // Serialize event to JSON
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish to exchange
        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _connection?.Dispose();
    }
}