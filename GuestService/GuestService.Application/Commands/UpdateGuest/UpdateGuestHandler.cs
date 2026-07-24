using MediatR;
using GuestService.Application.DTOs;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Application.Mappers;

namespace GuestService.Application.Commands.UpdateGuest;

public class UpdateGuestHandler : IRequestHandler<UpdateGuestCommand, GuestDto>
{
    private readonly IGuestRepository _guestRepository;
    private readonly ICachedPartyRepository _cachedPartyRepository;

    public UpdateGuestHandler(
        IGuestRepository guestRepository,
        ICachedPartyRepository cachedPartyRepository)
    {
        _guestRepository = guestRepository;
        _cachedPartyRepository = cachedPartyRepository;
    }

    public async Task<GuestDto> Handle(UpdateGuestCommand request, CancellationToken cancellationToken)
    {
        // Check if guest exists
        var existingGuest = await _guestRepository.GetByIdAsync(request.Id);

        if (existingGuest == null)
            throw new NotFoundException("Guest", request.Id);

        // Check if party exists in cache
        var party = await _cachedPartyRepository.GetByIdAsync(request.PartyId);

        if (party == null)
            throw new NotFoundException("Party", request.PartyId);

        // Update guest entity
        existingGuest.PartyId = request.PartyId;
        existingGuest.FullName = request.FullName;
        existingGuest.Age = request.Age;

        // Save via EF Core
        var updatedGuest = await _guestRepository.UpdateAsync(existingGuest);

        return GuestMapper.ToDto(updatedGuest);
    }
}