using MediatR;
using PartyService.Application.Events;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;

namespace PartyService.Application.Commands.DeleteParty;

public class DeletePartyHandler : IRequestHandler<DeletePartyCommand, bool>
{
    private readonly IPartyRepository _partyRepository;
    private readonly IOutboxEventPublisher _outboxEventPublisher; // Added because of the Outbox Pattern

    public DeletePartyHandler(IPartyRepository partyRepository, IOutboxEventPublisher outboxEventPublisher) // Added because of the Outbox Pattern
    {
        _partyRepository = partyRepository;
        _outboxEventPublisher = outboxEventPublisher; // Added because of the Outbox Pattern
    }

    public async Task<bool> Handle(DeletePartyCommand request, CancellationToken cancellationToken)
    {
        // Check if party exists
        var existingParty = await _partyRepository.GetByIdAsync(request.Id);

        if (existingParty == null)
            throw new NotFoundException("Party", request.Id);

        // Delete from database
        var deleted = await _partyRepository.DeleteAsync(request.Id);

        if (!deleted)
            throw new Exception("Failed to delete party");

        // Added because of the Outbox Pattern - Save event to outbox
        var partyDeletedEvent = new PartyDeletedEvent
        {
            PartyId = request.Id,
            OccurredAt = DateTime.UtcNow
        };

        await _outboxEventPublisher.PublishToOutboxAsync(partyDeletedEvent, "party.deleted");

        return true;
    }
}