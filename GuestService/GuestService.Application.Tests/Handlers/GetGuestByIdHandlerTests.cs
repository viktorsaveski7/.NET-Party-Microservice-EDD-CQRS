using Moq;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Application.Queries.GetGuestById;
using GuestService.Domain.Entities;

namespace GuestService.Application.Tests.Handlers;

public class GetGuestByIdHandlerTests
{
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly GetGuestByIdHandler _handler;

    public GetGuestByIdHandlerTests()
    {
        _handler = new GetGuestByIdHandler(_guestRepoMock.Object);
    }

    [Fact]
    public async Task Handle_GuestExists_ShouldReturnDto()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();
        var guest = new Guest
        {
            Id = guestId,
            PartyId = partyId,
            FullName = "John",
            Age = 10,
            CreatedAt = DateTime.UtcNow
        };

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(guest);

        var result = await _handler.Handle(new GetGuestByIdQuery(guestId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(guestId, result.Id);
        Assert.Equal("John", result.FullName);
        Assert.Equal(partyId, result.PartyId);
    }

    [Fact]
    public async Task Handle_GuestNotFound_ShouldThrowNotFoundException()
    {
        var guestId = Guid.NewGuid();

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetGuestByIdQuery(guestId), CancellationToken.None));
    }
}
