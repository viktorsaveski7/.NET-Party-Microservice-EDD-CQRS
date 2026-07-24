using Moq;
using GuestService.Application.EventHandlers;
using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.Tests.EventHandlers;

public class PartyCreatedEventHandlerTests
{
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock = new();
    private readonly Mock<ILogger<PartyCreatedEventHandler>> _loggerMock = new();
    private readonly PartyCreatedEventHandler _handler;

    public PartyCreatedEventHandlerTests()
    {
        _handler = new PartyCreatedEventHandler(_cachedPartyRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewParty_ShouldCreateCachedParty()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyCreatedEvent
        {
            PartyId = partyId,
            BirthdayChildName = "Alice",
            Title = "Birthday",
            BirthdayChildPhotoUrl = null,
            OccurredAt = DateTime.UtcNow
        };

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((CachedParty?)null);

        _cachedPartyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<CachedParty>()))
            .ReturnsAsync((CachedParty cp) => cp);

        await _handler.HandleAsync(@event);

        _cachedPartyRepoMock.Verify(r => r.CreateAsync(It.Is<CachedParty>(
            cp => cp.Id == partyId &&
                  cp.BirthdayChildName == "Alice" &&
                  cp.Title == "Birthday" &&
                  !cp.IsDeleted
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_PartyAlreadyExists_ShouldSkipCreation()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyCreatedEvent
        {
            PartyId = partyId,
            BirthdayChildName = "Alice",
            OccurredAt = DateTime.UtcNow
        };

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(new CachedParty { Id = partyId, BirthdayChildName = "Alice", LastUpdated = DateTime.UtcNow });

        await _handler.HandleAsync(@event);

        _cachedPartyRepoMock.Verify(r => r.CreateAsync(It.IsAny<CachedParty>()), Times.Never);
    }
}
