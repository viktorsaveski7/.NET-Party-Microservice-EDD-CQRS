using Moq;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;
using PartyService.Application.Queries.GetPartyById;
using PartyService.Domain.Entities;

namespace PartyService.Application.Tests.Handlers;

public class GetPartyByIdHandlerTests
{
    private readonly Mock<IPartyRepository> _partyRepoMock = new();
    private readonly GetPartyByIdHandler _handler;

    public GetPartyByIdHandlerTests()
    {
        _handler = new GetPartyByIdHandler(_partyRepoMock.Object);
    }

    [Fact]
    public async Task Handle_PartyExists_ShouldReturnDto()
    {
        var partyId = Guid.NewGuid();
        var party = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            Title = "My Party",
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(party);

        var result = await _handler.Handle(new GetPartyByIdQuery(partyId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(partyId, result.Id);
        Assert.Equal("Alice", result.BirthdayChildName);
    }

    [Fact]
    public async Task Handle_PartyNotFound_ShouldThrowNotFoundException()
    {
        var partyId = Guid.NewGuid();

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetPartyByIdQuery(partyId), CancellationToken.None));
    }
}
