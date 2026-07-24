using Moq;
using GuestService.Application.Commands.UpdateGuest;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;

namespace GuestService.Application.Tests.Handlers;

public class UpdateGuestHandlerTests
{
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock = new();
    private readonly UpdateGuestHandler _handler;

    public UpdateGuestHandlerTests()
    {
        _handler = new UpdateGuestHandler(_guestRepoMock.Object, _cachedPartyRepoMock.Object);
    }

    [Fact]
    public async Task Handle_GuestAndPartyExist_ShouldUpdateGuest()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();
        var command = new UpdateGuestCommand(guestId, partyId, "Updated Name", 15);

        var existingGuest = new Guest
        {
            Id = guestId,
            PartyId = Guid.NewGuid(),
            FullName = "Old Name",
            Age = 10,
            CreatedAt = DateTime.UtcNow
        };

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(existingGuest);

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(new CachedParty { Id = partyId, BirthdayChildName = "Alice", LastUpdated = DateTime.UtcNow });

        _guestRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Guest>()))
            .ReturnsAsync((Guest g) => g);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.FullName);
        Assert.Equal(15, result.Age);
        Assert.Equal(partyId, result.PartyId);
    }

    [Fact]
    public async Task Handle_GuestNotFound_ShouldThrowNotFoundException()
    {
        var guestId = Guid.NewGuid();
        var command = new UpdateGuestCommand(guestId, Guid.NewGuid(), "Name", 10);

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _guestRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldThrowNotFoundException()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();
        var command = new UpdateGuestCommand(guestId, partyId, "Name", 10);

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(new Guest { Id = guestId, FullName = "Old", CreatedAt = DateTime.UtcNow });

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((CachedParty?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
