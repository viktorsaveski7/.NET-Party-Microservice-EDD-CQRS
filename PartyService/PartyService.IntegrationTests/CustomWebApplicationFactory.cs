using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PartyService.Application.Interfaces;

namespace PartyService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IPartyRepository> PartyRepositoryMock { get; } = new();
    public Mock<IOutboxEventPublisher> OutboxEventPublisherMock { get; } = new();
    public Mock<IDistributedCache> DistributedCacheMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped(_ => PartyRepositoryMock.Object));
            services.Replace(ServiceDescriptor.Scoped(_ => OutboxEventPublisherMock.Object));
            services.Replace(ServiceDescriptor.Singleton(_ => DistributedCacheMock.Object));

            var outboxProcessorDescriptor = services
                .FirstOrDefault(d => d.ImplementationType?.Name == "OutboxProcessorService");
            if (outboxProcessorDescriptor is not null)
                services.Remove(outboxProcessorDescriptor);

            var eventPublisherDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IEventPublisher));
            if (eventPublisherDescriptor is not null)
                services.Remove(eventPublisherDescriptor);
        });
    }
}
