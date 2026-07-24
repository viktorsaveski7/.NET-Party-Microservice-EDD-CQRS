using MediatR;
using GuestService.Application.DTOs;

namespace GuestService.Application.Commands.UpdateGuest;

public record UpdateGuestCommand(
    Guid Id,
    Guid PartyId,
    string FullName,
    int? Age
) : IRequest<GuestDto>;