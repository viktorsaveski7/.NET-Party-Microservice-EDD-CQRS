// Added because of the Outbox Pattern
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;

namespace PartyService.Infrastructure.BackgroundServices;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private const int ProcessIntervalSeconds = 5; // Check every 5 seconds

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger,
        IEventPublisher eventPublisher)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(ProcessIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(batchSize: 10);

        foreach (var message in messages)
        {
            try
            {
                // Added because of the Outbox Pattern - Skip if in exponential backoff period
                if (ShouldSkipDueToBackoff(message))
                {
                    _logger.LogInformation("Skipping message {MessageId} due to exponential backoff (RetryCount: {RetryCount}, Next retry in ~{Seconds}s)",
                        message.Id, message.RetryCount, CalculateBackoffSeconds(message.RetryCount));
                    continue;
                }

                _logger.LogInformation("Publishing outbox message {MessageId} ({EventType})",
                    message.Id, message.EventType);

                // Publish to RabbitMQ using existing publisher
                await PublishRawEventAsync(message.EventData, message.RoutingKey);

                // Mark as processed
                await outboxRepository.MarkAsProcessedAsync(message.Id);

                _logger.LogInformation("Successfully published outbox message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} (Retry {RetryCount})",
                    message.Id, message.RetryCount + 1);
                await outboxRepository.IncrementRetryCountAsync(message.Id, ex.Message);
            }
        }
    }

    // Added because of the Outbox Pattern - Exponential backoff calculation
    private bool ShouldSkipDueToBackoff(OutboxMessage message)
    {
        if (message.RetryCount == 0) return false; // First attempt, no backoff

        var backoffSeconds = CalculateBackoffSeconds(message.RetryCount);
        var timeSinceCreation = DateTime.UtcNow - message.CreatedAt;

        // Calculate total expected wait time based on all previous retries
        var totalWaitTime = 0.0;
        for (int i = 1; i <= message.RetryCount; i++)
        {
            totalWaitTime += CalculateBackoffSeconds(i);
        }

        return timeSinceCreation.TotalSeconds < totalWaitTime;
    }

    // Added because of the Outbox Pattern - Exponential backoff formula
    private double CalculateBackoffSeconds(int retryCount)
    {
        // Exponential backoff: 5s, 10s, 20s, 40s, 80s, 160s, then cap at 300s (5 minutes)
        var backoffSeconds = Math.Pow(2, retryCount) * 5;
        return Math.Min(backoffSeconds, 300); // Max 5 minutes between retries
    }

    private async Task PublishRawEventAsync(string eventData, string routingKey)
    {
        // Use the existing RabbitMQ publisher
        var eventObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eventData);

        if (eventObject != null)
        {
            // This is a workaround - in production, you'd deserialize to actual event types
            await _eventPublisher.PublishAsync(eventObject, routingKey);
        }
    }
}