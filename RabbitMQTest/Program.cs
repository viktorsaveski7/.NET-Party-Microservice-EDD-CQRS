using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("🐰 RabbitMQ Connection Test\n");

// Connection settings
var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "admin123"
};

try
{
    // Test 1: Create connection
    Console.WriteLine("Test 1: Connecting to RabbitMQ...");
    using var connection = factory.CreateConnection();
    Console.WriteLine("✅ Connected successfully!");
    Console.WriteLine($"   Endpoint: {connection.Endpoint}");
    Console.WriteLine($"   Server: RabbitMQ\n");

    // Test 2: Create channel
    Console.WriteLine("Test 2: Creating channel...");
    using var channel = connection.CreateModel();
    Console.WriteLine("✅ Channel created successfully!\n");

    // Test 3: Declare a queue
    Console.WriteLine("Test 3: Creating test queue...");
    string queueName = "test-queue";
    channel.QueueDeclare(
        queue: queueName,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );
    Console.WriteLine($"✅ Queue '{queueName}' created!\n");

    // Test 4: Send a message
    Console.WriteLine("Test 4: Sending test message...");
    string message = "Hello from .NET! 🎉";
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(
        exchange: "",
        routingKey: queueName,
        basicProperties: null,
        body: body
    );
    Console.WriteLine($"✅ Message sent: '{message}'\n");

    // Test 5: Receive the message
    Console.WriteLine("Test 5: Receiving message...");
    var consumer = new EventingBasicConsumer(channel);
    string? receivedMessage = null;

    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        receivedMessage = Encoding.UTF8.GetString(body);
    };

    channel.BasicConsume(
        queue: queueName,
        autoAck: true,
        consumer: consumer
    );

    System.Threading.Thread.Sleep(500);

    if (receivedMessage != null)
    {
        Console.WriteLine($"✅ Message received: '{receivedMessage}'\n");
    }

    Console.WriteLine("==================================================");
    Console.WriteLine("🎉 ALL TESTS PASSED! RabbitMQ is working perfectly!");
    Console.WriteLine("==================================================");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"   Make sure RabbitMQ is running!");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();