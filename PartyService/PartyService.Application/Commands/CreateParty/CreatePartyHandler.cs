using MediatR;
using PartyService.Application.DTOs;
using PartyService.Application.Events;
using PartyService.Application.Interfaces;
using PartyService.Application.Mappers;
using PartyService.Domain.Entities;

namespace PartyService.Application.Commands.CreateParty;

public class CreatePartyHandler : IRequestHandler<CreatePartyCommand, PartyDto>
{
    private readonly IPartyRepository _partyRepository;
    private readonly IOutboxEventPublisher _outboxEventPublisher; // Added because of the Outbox Pattern

    public CreatePartyHandler(IPartyRepository partyRepository, IOutboxEventPublisher outboxEventPublisher) // Added because of the Outbox Pattern
    {
        _partyRepository = partyRepository;
        _outboxEventPublisher = outboxEventPublisher; // Added because of the Outbox Pattern
    }

    public async Task<PartyDto> Handle(CreatePartyCommand request, CancellationToken cancellationToken)
    {
        // Create entity
        var party = new Party
        {
            Id = Guid.NewGuid(),
            BirthdayChildName = request.BirthdayChildName,
            Title = request.Title,
            BirthdayChildPhotoUrl = request.BirthdayChildPhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        var savedParty = await _partyRepository.CreateAsync(party);

        // Added because of the Outbox Pattern - Save event to outbox instead of publishing directly
        var partyCreatedEvent = new PartyCreatedEvent
        {
            PartyId = savedParty.Id,
            BirthdayChildName = savedParty.BirthdayChildName,
            Title = savedParty.Title,
            BirthdayChildPhotoUrl = savedParty.BirthdayChildPhotoUrl,
            OccurredAt = DateTime.UtcNow
        };

        await _outboxEventPublisher.PublishToOutboxAsync(partyCreatedEvent, "party.created");

        // Map to DTO
        return PartyMapper.ToDto(savedParty);
    }
}