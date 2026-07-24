using MediatR;
using PartyService.Application.DTOs;

namespace PartyService.Application.Commands.CreateParty;

public record CreatePartyCommand(
    string BirthdayChildName,
    string? Title,
    string? BirthdayChildPhotoUrl
) : IRequest<PartyDto>;