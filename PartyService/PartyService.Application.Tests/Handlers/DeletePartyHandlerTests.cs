using Moq;
using PartyService.Application.Commands.DeleteParty;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;

namespace PartyService.Application.Tests.Handlers;

public class DeletePartyHandlerTests
{
    private readonly Mock<IPartyRepository> _partyRepoMock = new();
    private readonly Mock<IOutboxEventPublisher> _outboxMock = new();
    private readonly DeletePartyHandler _handler;

    public DeletePartyHandlerTests()
    {
        _handler = new DeletePartyHandler(_partyRepoMock.Object, _outboxMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingParty_ShouldDeleteAndPublishEvent()
    {
        var partyId = Guid.NewGuid();
        var command = new DeletePartyCommand(partyId);

        var existingParty = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(existingParty);

        _partyRepoMock
            .Setup(r => r.DeleteAsync(partyId))
            .ReturnsAsync(true);

        _outboxMock
            .Setup(o => o.PublishToOutboxAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _partyRepoMock.Verify(r => r.DeleteAsync(partyId), Times.Once);
        _outboxMock.Verify(o => o.PublishToOutboxAsync(It.IsAny<object>(), "party.deleted"), Times.Once);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldThrowNotFoundException()
    {
        var partyId = Guid.NewGuid();
        var command = new DeletePartyCommand(partyId);

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _partyRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeleteFailed_ShouldThrowException()
    {
        var partyId = Guid.NewGuid();
        var command = new DeletePartyCommand(partyId);

        var existingParty = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(existingParty);

        _partyRepoMock
            .Setup(r => r.DeleteAsync(partyId))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
}
