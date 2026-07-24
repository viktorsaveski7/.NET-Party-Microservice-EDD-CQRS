namespace GuestService.Infrastructure.EventConsumers;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "admin";
    public string Password { get; set; } = "admin123";
    public string ExchangeName { get; set; } = "party-events";
    public string QueueName { get; set; } = "guest-service-party-events";
}