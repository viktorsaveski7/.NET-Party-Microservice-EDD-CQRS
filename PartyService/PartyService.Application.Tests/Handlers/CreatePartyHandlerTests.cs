using Moq;
using PartyService.Application.Commands.CreateParty;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;

namespace PartyService.Application.Tests.Handlers;

public class CreatePartyHandlerTests
{
    private readonly Mock<IPartyRepository> _partyRepoMock = new();
    private readonly Mock<IOutboxEventPublisher> _outboxMock = new();
    private readonly CreatePartyHandler _handler;

    public CreatePartyHandlerTests()
    {
        _handler = new CreatePartyHandler(_partyRepoMock.Object, _outboxMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePartyAndPublishEvent()
    {
        var command = new CreatePartyCommand("Alice", "Birthday", null);
        var expectedParty = new Party
        {
            Id = Guid.NewGuid(),
            BirthdayChildName = "Alice",
            Title = "Birthday",
            BirthdayChildPhotoUrl = null,
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Party>()))
            .ReturnsAsync(expectedParty);

        _outboxMock
            .Setup(o => o.PublishToOutboxAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedParty.Id, result.Id);
        Assert.Equal("Alice", result.BirthdayChildName);
        Assert.Equal("Birthday", result.Title);

        _partyRepoMock.Verify(r => r.CreateAsync(It.IsAny<Party>()), Times.Once);
        _outboxMock.Verify(o => o.PublishToOutboxAsync(It.IsAny<object>(), "party.created"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPublishWithCorrectRoutingKey()
    {
        var command = new CreatePartyCommand("Bob", null, null);
        var expectedParty = new Party
        {
            Id = Guid.NewGuid(),
            BirthdayChildName = "Bob",
            Title = null,
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Party>()))
            .ReturnsAsync(expectedParty);

        _outboxMock
            .Setup(o => o.PublishToOutboxAsync(It.IsAny<object>(), "party.created"))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        _outboxMock.Verify(o => o.PublishToOutboxAsync(It.IsAny<object>(), "party.created"), Times.Once);
    }
}
