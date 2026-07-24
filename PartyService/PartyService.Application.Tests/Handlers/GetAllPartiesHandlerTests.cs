using Moq;
using PartyService.Application.Interfaces;
using PartyService.Application.Queries.GetAllParties;
using PartyService.Domain.Entities;

namespace PartyService.Application.Tests.Handlers;

public class GetAllPartiesHandlerTests
{
    private readonly Mock<IPartyRepository> _partyRepoMock = new();
    private readonly GetAllPartiesHandler _handler;

    public GetAllPartiesHandlerTests()
    {
        _handler = new GetAllPartiesHandler(_partyRepoMock.Object);
    }

    [Fact]
    public async Task Handle_PartiesExist_ShouldReturnMappedDtos()
    {
        var parties = new List<Party>
        {
            new()
            {
                Id = Guid.NewGuid(),
                BirthdayChildName = "Alice",
                Title = "Party 1",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                BirthdayChildName = "Bob",
                Title = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _partyRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(parties);

        var result = await _handler.Handle(new GetAllPartiesQuery(), CancellationToken.None);

        var dtos = result.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Alice", dtos[0].BirthdayChildName);
        Assert.Equal("Bob", dtos[1].BirthdayChildName);
    }

    [Fact]
    public async Task Handle_NoParties_ShouldReturnEmptyList()
    {
        _partyRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Party>());

        var result = await _handler.Handle(new GetAllPartiesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
