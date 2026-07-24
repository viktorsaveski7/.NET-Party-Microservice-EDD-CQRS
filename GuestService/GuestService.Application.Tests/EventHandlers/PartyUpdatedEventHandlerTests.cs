using Moq;
using GuestService.Application.EventHandlers;
using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.Tests.EventHandlers;

public class PartyUpdatedEventHandlerTests
{
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock = new();
    private readonly Mock<ILogger<PartyUpdatedEventHandler>> _loggerMock = new();
    private readonly PartyUpdatedEventHandler _handler;

    public PartyUpdatedEventHandlerTests()
    {
        _handler = new PartyUpdatedEventHandler(_cachedPartyRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_PartyExists_ShouldUpdateCachedParty()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyUpdatedEvent
        {
            PartyId = partyId,
            BirthdayChildName = "Alice Updated",
            Title = "New Title",
            OccurredAt = DateTime.UtcNow
        };

        var existingCachedParty = new CachedParty
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            Title = "Old Title",
            LastUpdated = DateTime.UtcNow
        };

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(existingCachedParty);

        _cachedPartyRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CachedParty>()))
            .ReturnsAsync((CachedParty cp) => cp);

        await _handler.HandleAsync(@event);

        _cachedPartyRepoMock.Verify(r => r.UpdateAsync(It.Is<CachedParty>(
            cp => cp.BirthdayChildName == "Alice Updated" && cp.Title == "New Title"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldSkipUpdate()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyUpdatedEvent
        {
            PartyId = partyId,
            BirthdayChildName = "Alice",
            OccurredAt = DateTime.UtcNow
        };

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((CachedParty?)null);

        await _handler.HandleAsync(@event);

        _cachedPartyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<CachedParty>()), Times.Never);
    }
}
