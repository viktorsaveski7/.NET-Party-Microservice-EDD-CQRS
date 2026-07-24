using Moq;
using GuestService.Application.EventHandlers;
using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.Tests.EventHandlers;

public class PartyDeletedEventHandlerTests
{
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock = new();
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly Mock<ILogger<PartyDeletedEventHandler>> _loggerMock = new();
    private readonly PartyDeletedEventHandler _handler;

    public PartyDeletedEventHandlerTests()
    {
        _handler = new PartyDeletedEventHandler(
            _cachedPartyRepoMock.Object,
            _guestRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteGuestsAndMarkPartyAsDeleted()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyDeletedEvent
        {
            PartyId = partyId,
            OccurredAt = DateTime.UtcNow
        };

        _guestRepoMock
            .Setup(r => r.DeleteGuestsByPartyIdAsync(partyId))
            .Returns(Task.CompletedTask);

        _cachedPartyRepoMock
            .Setup(r => r.MarkAsDeletedAsync(partyId))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(@event);

        _guestRepoMock.Verify(r => r.DeleteGuestsByPartyIdAsync(partyId), Times.Once);
        _cachedPartyRepoMock.Verify(r => r.MarkAsDeletedAsync(partyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteGuestsBeforeMarkingPartyDeleted()
    {
        var partyId = Guid.NewGuid();
        var @event = new PartyDeletedEvent { PartyId = partyId, OccurredAt = DateTime.UtcNow };

        var callOrder = new List<string>();

        _guestRepoMock
            .Setup(r => r.DeleteGuestsByPartyIdAsync(partyId))
            .Callback(() => callOrder.Add("DeleteGuests"))
            .Returns(Task.CompletedTask);

        _cachedPartyRepoMock
            .Setup(r => r.MarkAsDeletedAsync(partyId))
            .Callback(() => callOrder.Add("MarkDeleted"))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(@event);

        Assert.Equal("DeleteGuests", callOrder[0]);
        Assert.Equal("MarkDeleted", callOrder[1]);
    }
}
