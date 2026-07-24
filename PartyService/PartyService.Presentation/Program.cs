using DbUp;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using PartyService.Application.Behaviors;
using PartyService.Application.Commands.CreateParty;
using PartyService.Application.Interfaces;
using PartyService.Infrastructure.BackgroundServices;
using PartyService.Infrastructure.Database;
using PartyService.Infrastructure.EventPublishing;
using PartyService.Infrastructure.Repositories;
using PartyService.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR with Validation Behavior
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreatePartyCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); //  Add validation pipeline
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreatePartyValidator>();

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

// Register Redis distributed cache
var redisConnectionString = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis connection string 'Redis:ConnectionString' not found.");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

// Register repositories
builder.Services.AddScoped<PartyRepository>();
builder.Services.AddScoped<IPartyRepository>(sp =>
    new CachedPartyRepository(
        sp.GetRequiredService<PartyRepository>(),
        sp.GetRequiredService<IDistributedCache>()));

builder.Services.AddScoped<IOutboxRepository, OutboxRepository>(); // Added because of the Outbox Pattern

// Register event publishers
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>(); // Keep for background worker
builder.Services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>(); // Added because of the Outbox Pattern

// Register Outbox Background Service
builder.Services.AddHostedService<OutboxProcessorService>(); // Added because of the Outbox Pattern

var rabbitMqSettings = new RabbitMqSettings();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqSettings);
builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
//  DbUp: Auto-run SQL migrations
// ============================================
Console.WriteLine("Running database migrations...");

try
{
    var upgrader = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(typeof(PartyRepository).Assembly)
        .WithTransaction()
        .LogToConsole()
        .Build();

    var result = upgrader.PerformUpgrade();

    if (!result.Successful)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Migration failed: {result.Error}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database migrations completed successfully!");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Migration error: {ex.Message}");
    Console.ResetColor();
}

// ============================================
// Configure the HTTP request pipeline
// ============================================

// Add Global Exception Middleware (FIRST!)
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Application started successfully!");
app.Run();

public partial class Program { }