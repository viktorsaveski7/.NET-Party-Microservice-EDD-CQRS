using GuestService.Application.EventHandlers;
using GuestService.Application.Events;
using GuestService.Infrastructure.EventConsumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GuestService.Infrastructure.EventConsumers;
public class PartyEventConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PartyEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public PartyEventConsumer(
        RabbitMqSettings settings,
        IServiceProvider serviceProvider,
        ILogger<PartyEventConsumer> logger)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Party Event Consumer starting...");

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Declare exchange (same as Party Service)
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            // Declare queue
            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            // Bind queue to exchange with routing patterns
            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: "party.created",
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: "party.updated",
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: "party.deleted",
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Successfully connected to RabbitMQ and bound to exchange");

            // Create consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

                try
                {
                    await ProcessMessageAsync(routingKey, message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Listening for party events...");

            // Keep running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Party Event Consumer");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string routingKey, string message)
    {
        using var scope = _serviceProvider.CreateScope();

        switch (routingKey)
        {
            case "party.created":
                var createdEvent = JsonSerializer.Deserialize<PartyCreatedEvent>(message);
                if (createdEvent != null)
                {
                    var createdHandler = scope.ServiceProvider.GetRequiredService<PartyCreatedEventHandler>();
                    await createdHandler.HandleAsync(createdEvent);
                }
                break;

            case "party.updated":
                var updatedEvent = JsonSerializer.Deserialize<PartyUpdatedEvent>(message);
                if (updatedEvent != null)
                {
                    var updatedHandler = scope.ServiceProvider.GetRequiredService<PartyUpdatedEventHandler>();
                    await updatedHandler.HandleAsync(updatedEvent);
                }
                break;

            case "party.deleted":
                var deletedEvent = JsonSerializer.Deserialize<PartyDeletedEvent>(message);
                if (deletedEvent != null)
                {
                    var deletedHandler = scope.ServiceProvider.GetRequiredService<PartyDeletedEventHandler>();
                    await deletedHandler.HandleAsync(deletedEvent);
                }
                break;

            default:
                _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                break;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Party Event Consumer stopping...");

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}