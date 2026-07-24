using MediatR;
using GuestService.Application.DTOs;

namespace GuestService.Application.Queries.GetAllGuests;

public record GetAllGuestsQuery : IRequest<IEnumerable<GuestDto>>;