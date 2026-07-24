using MediatR;
using PartyService.Application.DTOs;
using PartyService.Application.Events;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;
using PartyService.Application.Mappers;
using PartyService.Domain.Entities;

namespace PartyService.Application.Commands.UpdateParty;

public class UpdatePartyHandler : IRequestHandler<UpdatePartyCommand, PartyDto>
{
    private readonly IPartyRepository _partyRepository;
    private readonly IOutboxEventPublisher _outboxEventPublisher; // Added because of the Outbox Pattern

    public UpdatePartyHandler(IPartyRepository partyRepository, IOutboxEventPublisher outboxEventPublisher) // Added because of the Outbox Pattern
    {
        _partyRepository = partyRepository;
        _outboxEventPublisher = outboxEventPublisher; // Added because of the Outbox Pattern
    }

    public async Task<PartyDto> Handle(UpdatePartyCommand request, CancellationToken cancellationToken)
    {
        // Check if party exists
        var existingParty = await _partyRepository.GetByIdAsync(request.Id);

        if (existingParty == null)
            throw new NotFoundException("Party", request.Id);

        // Create updated party entity
        var updatedParty = new Party
        {
            Id = request.Id,
            BirthdayChildName = request.BirthdayChildName,
            Title = request.Title,
            BirthdayChildPhotoUrl = request.BirthdayChildPhotoUrl,
            CreatedAt = existingParty.CreatedAt
        };

        // Save to database
        var result = await _partyRepository.UpdateAsync(updatedParty);

        // Added because of the Outbox Pattern - Save event to outbox
        var partyUpdatedEvent = new PartyUpdatedEvent
        {
            PartyId = result.Id,
            BirthdayChildName = result.BirthdayChildName,
            Title = result.Title,
            BirthdayChildPhotoUrl = result.BirthdayChildPhotoUrl,
            OccurredAt = DateTime.UtcNow
        };

        await _outboxEventPublisher.PublishToOutboxAsync(partyUpdatedEvent, "party.updated");

        return PartyMapper.ToDto(result);
    }
}