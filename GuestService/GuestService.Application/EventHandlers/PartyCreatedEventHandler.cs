using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.EventHandlers;

public class PartyCreatedEventHandler
{
    private readonly ICachedPartyRepository _cachedPartyRepository;
    private readonly ILogger<PartyCreatedEventHandler> _logger;

    public PartyCreatedEventHandler(
        ICachedPartyRepository cachedPartyRepository,
        ILogger<PartyCreatedEventHandler> logger)
    {
        _cachedPartyRepository = cachedPartyRepository;
        _logger = logger;
    }

    public async Task HandleAsync(PartyCreatedEvent @event)
    {
        _logger.LogInformation("Handling PartyCreatedEvent for Party {PartyId}", @event.PartyId);

        try
        {
            // Check if party already exists
            var existingParty = await _cachedPartyRepository.GetByIdAsync(@event.PartyId);

            if (existingParty != null)
            {
                _logger.LogWarning("Party {PartyId} already exists in cache", @event.PartyId);
                return;
            }

            // Create cached party
            var cachedParty = new CachedParty
            {
                Id = @event.PartyId,
                BirthdayChildName = @event.BirthdayChildName,
                Title = @event.Title,
                BirthdayChildPhotoUrl = @event.BirthdayChildPhotoUrl,
                IsDeleted = false,
                LastUpdated = DateTime.UtcNow
            };

            await _cachedPartyRepository.CreateAsync(cachedParty);

            _logger.LogInformation("Successfully cached Party {PartyId}", @event.PartyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PartyCreatedEvent for Party {PartyId}", @event.PartyId);
            throw;
        }
    }
}