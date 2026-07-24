using MediatR;
using GuestService.Application.DTOs;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Application.Mappers;
using GuestService.Domain.Entities;

namespace GuestService.Application.Commands.CreateGuest;

public class CreateGuestHandler : IRequestHandler<CreateGuestCommand, GuestDto>
{
    private readonly IGuestRepository _guestRepository;
    private readonly ICachedPartyRepository _cachedPartyRepository;

    public CreateGuestHandler(
        IGuestRepository guestRepository,
        ICachedPartyRepository cachedPartyRepository)
    {
        _guestRepository = guestRepository;
        _cachedPartyRepository = cachedPartyRepository;
    }

    public async Task<GuestDto> Handle(CreateGuestCommand request, CancellationToken cancellationToken)
    {
        // Check if party exists in cache
        var party = await _cachedPartyRepository.GetByIdAsync(request.PartyId);

        if (party == null)
            throw new NotFoundException("Party", request.PartyId);

        // Create guest entity
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            PartyId = request.PartyId,
            FullName = request.FullName,
            Age = request.Age,
            CreatedAt = DateTime.UtcNow
        };

        // Save via EF Core repository
        var savedGuest = await _guestRepository.CreateAsync(guest);

        // Map to DTO
        return GuestMapper.ToDto(savedGuest);
    }
}