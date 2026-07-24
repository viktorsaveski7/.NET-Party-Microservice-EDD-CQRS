using Moq;
using GuestService.Application.Interfaces;
using GuestService.Application.Queries.GetAllGuests;
using GuestService.Domain.Entities;

namespace GuestService.Application.Tests.Handlers;

public class GetAllGuestsHandlerTests
{
    private readonly Mock<IGuestRepository> _guestRepoMock = new();
    private readonly GetAllGuestsHandler _handler;

    public GetAllGuestsHandlerTests()
    {
        _handler = new GetAllGuestsHandler(_guestRepoMock.Object);
    }

    [Fact]
    public async Task Handle_GuestsExist_ShouldReturnMappedDtos()
    {
        var guests = new List<Guest>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PartyId = Guid.NewGuid(),
                FullName = "John",
                Age = 10,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                PartyId = Guid.NewGuid(),
                FullName = "Jane",
                Age = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        _guestRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(guests);

        var result = await _handler.Handle(new GetAllGuestsQuery(), CancellationToken.None);

        var dtos = result.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("John", dtos[0].FullName);
        Assert.Null(dtos[1].Age);
    }

    [Fact]
    public async Task Handle_NoGuests_ShouldReturnEmptyList()
    {
        _guestRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Guest>());

        var result = await _handler.Handle(new GetAllGuestsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
