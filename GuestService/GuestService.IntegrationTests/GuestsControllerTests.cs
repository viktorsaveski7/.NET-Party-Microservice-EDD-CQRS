using System.Net;
using System.Net.Http.Json;
using GuestService.Application.DTOs;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using Moq;

namespace GuestService.IntegrationTests;

public class GuestsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<IGuestRepository> _guestRepoMock;
    private readonly Mock<ICachedPartyRepository> _cachedPartyRepoMock;

    public GuestsControllerTests(CustomWebApplicationFactory factory)
    {
        _guestRepoMock = factory.GuestRepositoryMock;
        _cachedPartyRepoMock = factory.CachedPartyRepositoryMock;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGuest_ValidRequest_ReturnsCreated201()
    {
        var partyId = Guid.NewGuid();
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

        var content = JsonContent.Create(new
        {
            PartyId = partyId,
            FullName = "John Doe",
            Age = 10
        });

        var response = await _client.PostAsync("/api/guests", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GuestDto>();
        Assert.NotNull(dto);
        Assert.Equal("John Doe", dto.FullName);
        Assert.Equal(10, dto.Age);
    }

    [Fact]
    public async Task CreateGuest_PartyNotFound_ReturnsNotFound404()
    {
        var partyId = Guid.NewGuid();

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync((CachedParty?)null);

        var content = JsonContent.Create(new
        {
            PartyId = partyId,
            FullName = "John Doe",
            Age = 10
        });

        var response = await _client.PostAsync("/api/guests", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateGuest_InvalidRequest_ReturnsUnprocessableEntity422()
    {
        var content = JsonContent.Create(new
        {
            PartyId = Guid.Empty,
            FullName = "",
            Age = (int?)null
        });

        var response = await _client.PostAsync("/api/guests", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetGuestById_ExistingGuest_ReturnsOk200()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();
        var guest = new Guest
        {
            Id = guestId,
            PartyId = partyId,
            FullName = "John Doe",
            Age = 10,
            CreatedAt = DateTime.UtcNow
        };

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(guest);

        var response = await _client.GetAsync($"/api/guests/{guestId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GuestDto>();
        Assert.NotNull(dto);
        Assert.Equal(guestId, dto.Id);
        Assert.Equal("John Doe", dto.FullName);
    }

    [Fact]
    public async Task GetGuestById_NonExistingGuest_ReturnsNotFound404()
    {
        var guestId = Guid.NewGuid();

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        var response = await _client.GetAsync($"/api/guests/{guestId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllGuests_ReturnsOk200()
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
                Age = 8,
                CreatedAt = DateTime.UtcNow
            }
        };

        _guestRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(guests);

        var response = await _client.GetAsync("/api/guests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dtos = await response.Content.ReadFromJsonAsync<List<GuestDto>>();
        Assert.NotNull(dtos);
        Assert.Equal(2, dtos.Count);
    }

    [Fact]
    public async Task UpdateGuest_ExistingGuestAndParty_ReturnsOk200()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();

        var existingGuest = new Guest
        {
            Id = guestId,
            PartyId = Guid.NewGuid(),
            FullName = "Old Name",
            Age = 5,
            CreatedAt = DateTime.UtcNow
        };

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(existingGuest);

        _cachedPartyRepoMock
            .Setup(r => r.GetByIdAsync(partyId))
            .ReturnsAsync(new CachedParty
            {
                Id = partyId,
                BirthdayChildName = "Alice",
                LastUpdated = DateTime.UtcNow
            });

        _guestRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Guest>()))
            .ReturnsAsync((Guest g) => g);

        var content = JsonContent.Create(new
        {
            Id = guestId,
            PartyId = partyId,
            FullName = "Updated Name",
            Age = 12
        });

        var response = await _client.PutAsync($"/api/guests/{guestId}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GuestDto>();
        Assert.NotNull(dto);
        Assert.Equal("Updated Name", dto.FullName);
        Assert.Equal(12, dto.Age);
    }

    [Fact]
    public async Task UpdateGuest_GuestNotFound_ReturnsNotFound404()
    {
        var guestId = Guid.NewGuid();
        var partyId = Guid.NewGuid();

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        var content = JsonContent.Create(new
        {
            Id = guestId,
            PartyId = partyId,
            FullName = "Updated Name",
            Age = 12
        });

        var response = await _client.PutAsync($"/api/guests/{guestId}", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGuest_IdMismatch_ReturnsBadRequest400()
    {
        var guestId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var content = JsonContent.Create(new
        {
            Id = differentId,
            PartyId = Guid.NewGuid(),
            FullName = "Updated Name",
            Age = 12
        });

        var response = await _client.PutAsync($"/api/guests/{guestId}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGuest_ExistingGuest_ReturnsNoContent204()
    {
        var guestId = Guid.NewGuid();

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync(new Guest
            {
                Id = guestId,
                PartyId = Guid.NewGuid(),
                FullName = "John",
                CreatedAt = DateTime.UtcNow
            });

        _guestRepoMock
            .Setup(r => r.DeleteAsync(guestId))
            .ReturnsAsync(true);

        var response = await _client.DeleteAsync($"/api/guests/{guestId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGuest_NonExistingGuest_ReturnsNotFound404()
    {
        var guestId = Guid.NewGuid();

        _guestRepoMock
            .Setup(r => r.GetByIdAsync(guestId))
            .ReturnsAsync((Guest?)null);

        var response = await _client.DeleteAsync($"/api/guests/{guestId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
