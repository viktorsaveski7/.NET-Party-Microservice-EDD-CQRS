using Moq;
using GuestService.Application.Commands.DeleteGuest;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;

namespace GuestService.Application.Tests.Handlers;

public class DeleteGuestHandlerTests
{
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly DeleteGuestHandler _handler;

    public DeleteGuestHandlerTests()
    {
        _handler = new DeleteGuestHandler(_guestRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingGuest_ShouldDeleteAndReturnTrue()
    {
        var guestId = Guid.NewGuid();
        var command = new DeleteGuestCommand(guestId);

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(new Guest { Id = guestId, FullName = "John", CreatedAt = DateTime.UtcNow });

        _guestRepoMock
            .Setup(r => r.DeleteAsync(guestId))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _guestRepoMock.Verify(r => r.DeleteAsync(guestId), Times.Once);
    }

    [Fact]
    public async Task Handle_GuestNotFound_ShouldThrowNotFoundException()
    {
        var guestId = Guid.NewGuid();
        var command = new DeleteGuestCommand(guestId);

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeleteFailed_ShouldThrowException()
    {
        var guestId = Guid.NewGuid();
        var command = new DeleteGuestCommand(guestId);

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(new Guest { Id = guestId, FullName = "John", CreatedAt = DateTime.UtcNow });

        _guestRepoMock
            .Setup(r => r.DeleteAsync(guestId))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
}
