using FluentValidation;
using GuestService.Application.Behaviors;
using GuestService.Application.Commands.CreateGuest;
using GuestService.Application.EventHandlers;
using GuestService.Application.Interfaces;
using GuestService.Infrastructure.EventConsumers;
using GuestService.Infrastructure.Persistence;
using GuestService.Infrastructure.Repositories;
using GuestService.Presentation.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR with Validation Behavior
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateGuestCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateGuestCommand>();

// Add EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<GuestDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<ICachedPartyRepository, CachedPartyRepository>();

// Register Event Handlers
builder.Services.AddScoped<PartyCreatedEventHandler>();
builder.Services.AddScoped<PartyUpdatedEventHandler>();
builder.Services.AddScoped<PartyDeletedEventHandler>();

// Register RabbitMQ Settings
var rabbitMqSettings = new RabbitMqSettings();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqSettings);
builder.Services.AddSingleton(rabbitMqSettings);

// Register RabbitMQ Consumer (Background Service)
builder.Services.AddHostedService<PartyEventConsumer>();

var app = builder.Build();

// Add Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Guest Service started successfully on http://localhost:5002!");
app.Run();

public partial class Program { }