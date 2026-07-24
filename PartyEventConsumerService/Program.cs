using GuestService.Application.EventHandlers;
using GuestService.Application.Interfaces;
using GuestService.Infrastructure.EventConsumers;
using GuestService.Infrastructure.Persistence;
using GuestService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<GuestDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<ICachedPartyRepository, CachedPartyRepository>();

builder.Services.AddScoped<PartyCreatedEventHandler>();
builder.Services.AddScoped<PartyUpdatedEventHandler>();
builder.Services.AddScoped<PartyDeletedEventHandler>();

var rabbitMqSettings = new RabbitMqSettings();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMqSettings);
builder.Services.AddSingleton(rabbitMqSettings);

builder.Services.AddHostedService<PartyEventConsumer>();

var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<GuestDbContext>();
await db.Database.MigrateAsync();

await host.RunAsync();
