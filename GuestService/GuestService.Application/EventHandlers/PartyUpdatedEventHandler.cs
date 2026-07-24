using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.EventHandlers;

public class PartyUpdatedEventHandler
{
    private readonly ICachedPartyRepository _cachedPartyRepository;
    private readonly ILogger<PartyUpdatedEventHandler> _logger;

    public PartyUpdatedEventHandler(
        ICachedPartyRepository cachedPartyRepository,
        ILogger<PartyUpdatedEventHandler> logger)
    {
        _cachedPartyRepository = cachedPartyRepository;
        _logger = logger;
    }

    public async Task HandleAsync(PartyUpdatedEvent @event)
    {
        _logger.LogInformation("Handling PartyUpdatedEvent for Party {PartyId}", @event.PartyId);

        try
        {
            var cachedParty = await _cachedPartyRepository.GetByIdAsync(@event.PartyId);

            if (cachedParty == null)
            {
                _logger.LogWarning("Party {PartyId} not found in cache, cannot update", @event.PartyId);
                return;
            }

            // Update cached party
            cachedParty.BirthdayChildName = @event.BirthdayChildName;
            cachedParty.Title = @event.Title;
            cachedParty.BirthdayChildPhotoUrl = @event.BirthdayChildPhotoUrl;
            cachedParty.LastUpdated = DateTime.UtcNow;

            await _cachedPartyRepository.UpdateAsync(cachedParty);

            _logger.LogInformation("Successfully updated cached Party {PartyId}", @event.PartyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PartyUpdatedEvent for Party {PartyId}", @event.PartyId);
            throw;
        }
    }
}