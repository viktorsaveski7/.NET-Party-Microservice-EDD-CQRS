using MediatR;
using GuestService.Application.DTOs;

namespace GuestService.Application.Commands.CreateGuest;

public record CreateGuestCommand(
    Guid PartyId,
    string FullName,
    int? Age
) : IRequest<GuestDto>;