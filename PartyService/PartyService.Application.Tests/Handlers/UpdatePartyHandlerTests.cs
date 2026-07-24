using Moq;
using PartyService.Application.Commands.UpdateParty;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;

namespace PartyService.Application.Tests.Handlers;

public class UpdatePartyHandlerTests
{
    private readonly Mock<IPartyRepository> _partyRepoMock = new();
    private readonly Mock<IOutboxEventPublisher> _outboxMock = new();
    private readonly UpdatePartyHandler _handler;

    public UpdatePartyHandlerTests()
    {
        _handler = new UpdatePartyHandler(_partyRepoMock.Object, _outboxMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingParty_ShouldUpdateAndPublishEvent()
    {
        var partyId = Guid.NewGuid();
        var command = new UpdatePartyCommand(partyId, "Alice Updated", "New Title", null);

        var existingParty = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            Title = "Old Title",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedParty = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice Updated",
            Title = "New Title",
            CreatedAt = existingParty.CreatedAt
        };

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(existingParty);

        _partyRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Party>()))
            .ReturnsAsync(updatedParty);

        _outboxMock
            .Setup(o => o.PublishToOutboxAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Alice Updated", result.BirthdayChildName);
        Assert.Equal("New Title", result.Title);
        _outboxMock.Verify(o => o.PublishToOutboxAsync(It.IsAny<object>(), "party.updated"), Times.Once);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldThrowNotFoundException()
    {
        var partyId = Guid.NewGuid();
        var command = new UpdatePartyCommand(partyId, "Alice", null, null);

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _partyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Party>()), Times.Never);
    }
}
