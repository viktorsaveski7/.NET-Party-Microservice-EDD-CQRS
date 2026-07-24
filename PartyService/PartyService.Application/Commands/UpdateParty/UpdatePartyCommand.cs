using MediatR;
using PartyService.Application.DTOs;

namespace PartyService.Application.Commands.UpdateParty;

public record UpdatePartyCommand(
    Guid Id,
    string BirthdayChildName,
    string? Title,
    string? BirthdayChildPhotoUrl
) : IRequest<PartyDto>;