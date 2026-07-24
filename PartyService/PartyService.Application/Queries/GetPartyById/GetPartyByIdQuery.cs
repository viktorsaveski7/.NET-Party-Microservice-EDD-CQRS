using MediatR;
using PartyService.Application.DTOs;

namespace PartyService.Application.Queries.GetPartyById;

public record GetPartyByIdQuery(Guid Id) : IRequest<PartyDto>;