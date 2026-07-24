using MediatR;
using GuestService.Application.DTOs;

namespace GuestService.Application.Queries.GetGuestById;

public record GetGuestByIdQuery(Guid Id) : IRequest<GuestDto>;