using System.Net;
using System.Net.Http.Json;
using PartyService.Application.DTOs;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;
using Moq;

namespace PartyService.IntegrationTests;

public class PartiesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartyRepository> _partyRepoMock;
    private readonly Mock<IOutboxEventPublisher> _outboxMock;

    public PartiesControllerTests(CustomWebApplicationFactory factory)
    {
        _partyRepoMock = factory.PartyRepositoryMock;
        _outboxMock = factory.OutboxEventPublisherMock;
        _outboxMock.Setup(o => o.PublishToOutboxAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateParty_ValidRequest_ReturnsCreated201()
    {
        var expectedParty = new Party
        {
            Id = Guid.NewGuid(),
            BirthdayChildName = "Alice",
            Title = "Birthday Bash",
            CreatedAt = DateTime.UtcNow
        };

        _partyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Party>()))
            .ReturnsAsync(expectedParty);

        var content = JsonContent.Create(new
        {
            BirthdayChildName = "Alice",
            Title = "Birthday Bash"
        });

        var response = await _client.PostAsync("/api/parties", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<PartyDto>();
        Assert.NotNull(dto);
        Assert.Equal("Alice", dto.BirthdayChildName);
        Assert.Equal("Birthday Bash", dto.Title);
    }

    [Fact]
    public async Task CreateParty_InvalidRequest_ReturnsUnprocessableEntity422()
    {
        var content = JsonContent.Create(new
        {
            BirthdayChildName = "",
            Title = (string?)null
        });

        var response = await _client.PostAsync("/api/parties", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetPartyById_ExistingParty_ReturnsOk200()
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

        var response = await _client.GetAsync($"/api/parties/{partyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<PartyDto>();
        Assert.NotNull(dto);
        Assert.Equal(partyId, dto.Id);
        Assert.Equal("Alice", dto.BirthdayChildName);
    }

    [Fact]
    public async Task GetPartyById_NonExistingParty_ReturnsNotFound404()
    {
        var partyId = Guid.NewGuid();

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        var response = await _client.GetAsync($"/api/parties/{partyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllParties_ReturnsOk200()
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
                Title = "Party 2",
                CreatedAt = DateTime.UtcNow
            }
        };

        _partyRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(parties);

        var response = await _client.GetAsync("/api/parties");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dtos = await response.Content.ReadFromJsonAsync<List<PartyDto>>();
        Assert.NotNull(dtos);
        Assert.Equal(2, dtos.Count);
    }

    [Fact]
    public async Task UpdateParty_ExistingParty_ReturnsOk200()
    {
        var partyId = Guid.NewGuid();
        var existingParty = new Party
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            Title = "Old Title",
            CreatedAt = DateTime.UtcNow
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

        var content = JsonContent.Create(new
        {
            Id = partyId,
            BirthdayChildName = "Alice Updated",
            Title = "New Title"
        });

        var response = await _client.PutAsync($"/api/parties/{partyId}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<PartyDto>();
        Assert.NotNull(dto);
        Assert.Equal("Alice Updated", dto.BirthdayChildName);
    }

    [Fact]
    public async Task UpdateParty_NonExistingParty_ReturnsNotFound404()
    {
        var partyId = Guid.NewGuid();

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        var content = JsonContent.Create(new
        {
            Id = partyId,
            BirthdayChildName = "Alice",
            Title = "New Title"
        });

        var response = await _client.PutAsync($"/api/parties/{partyId}", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateParty_IdMismatch_ReturnsBadRequest400()
    {
        var partyId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var content = JsonContent.Create(new
        {
            Id = differentId,
            BirthdayChildName = "Alice",
            Title = "New Title"
        });

        var response = await _client.PutAsync($"/api/parties/{partyId}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteParty_ExistingParty_ReturnsNoContent204()
    {
        var partyId = Guid.NewGuid();
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

        var response = await _client.DeleteAsync($"/api/parties/{partyId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteParty_NonExistingParty_ReturnsNotFound404()
    {
        var partyId = Guid.NewGuid();

        _partyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((Party?)null);

        var response = await _client.DeleteAsync($"/api/parties/{partyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
