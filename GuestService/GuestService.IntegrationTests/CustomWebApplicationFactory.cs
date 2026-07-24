using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using GuestService.Application.Interfaces;

namespace GuestService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IGuestRepository> GuestRepositoryMock { get; } = new();
    public Mock<ICachedPartyRepository> CachedPartyRepositoryMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped(_ => GuestRepositoryMock.Object));
            services.Replace(ServiceDescriptor.Scoped(_ => CachedPartyRepositoryMock.Object));

            var consumerDescriptor = services
                .FirstOrDefault(d => d.ImplementationType?.Name == "PartyEventConsumer");
            if (consumerDescriptor is not null)
                services.Remove(consumerDescriptor);
        });
    }
}
