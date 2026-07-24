using GuestService.Application.Events;
using GuestService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GuestService.Application.EventHandlers;

public class PartyDeletedEventHandler
{
    private readonly ICachedPartyRepository _cachedPartyRepository;
    private readonly IGuestRepository _guestRepository; // ← NEW
    private readonly ILogger<PartyDeletedEventHandler> _logger;

    public PartyDeletedEventHandler(
        ICachedPartyRepository cachedPartyRepository,
        IGuestRepository guestRepository, // ← NEW
        ILogger<PartyDeletedEventHandler> logger)
    {
        _cachedPartyRepository = cachedPartyRepository;
        _guestRepository = guestRepository; // ← NEW
        _logger = logger;
    }

    public async Task HandleAsync(PartyDeletedEvent @event)
    {
        _logger.LogInformation("Handling PartyDeletedEvent for Party {PartyId}", @event.PartyId);

        try
        {
            // Delete all guests for this party (HARD DELETE)
            await _guestRepository.DeleteGuestsByPartyIdAsync(@event.PartyId);

            // Mark party as deleted (SOFT DELETE)
            await _cachedPartyRepository.MarkAsDeletedAsync(@event.PartyId);

            _logger.LogInformation("Successfully deleted guests and marked Party {PartyId} as deleted", @event.PartyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PartyDeletedEvent for Party {PartyId}", @event.PartyId);
            throw;
        }
    }
}