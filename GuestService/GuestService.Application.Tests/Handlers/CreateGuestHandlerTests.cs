using Moq;
using GuestService.Application.Commands.CreateGuest;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;

namespace GuestService.Application.Tests.Handlers;

public class CreateGuestHandlerTests
{
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock = new();
    private readonly CreateGuestHandler _handler;

    public CreateGuestHandlerTests()
    {
        _handler = new CreateGuestHandler(_guestRepoMock.Object, _cachedPartyRepoMock.Object);
    }

    [Fact]
    public async Task Handle_PartyExists_ShouldCreateGuest()
    {
        var partyId = Guid.NewGuid();
        var command = new CreateGuestCommand(partyId, "John Doe", 10);

        var cachedParty = new CachedParty
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            IsDeleted = false,
            LastUpdated = DateTime.UtcNow
        };

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(cachedParty);

        _guestRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Guest>()))
            .ReturnsAsync((Guest g) => g);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal(10, result.Age);
        Assert.Equal(partyId, result.PartyId);

        _guestRepoMock.Verify(r => r.CreateAsync(It.IsAny<Guest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldThrowNotFoundException()
    {
        var partyId = Guid.NewGuid();
        var command = new CreateGuestCommand(partyId, "John Doe", 10);

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((CachedParty?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _guestRepoMock.Verify(r => r.CreateAsync(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommandWithoutAge_ShouldCreateGuest()
    {
        var partyId = Guid.NewGuid();
        var command = new CreateGuestCommand(partyId, "Jane Doe", null);

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(new CachedParty { Id = partyId, BirthdayChildName = "Alice", LastUpdated = DateTime.UtcNow });

        _guestRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Guest>()))
            .ReturnsAsync((Guest g) => g);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Null(result.Age);
    }
}
