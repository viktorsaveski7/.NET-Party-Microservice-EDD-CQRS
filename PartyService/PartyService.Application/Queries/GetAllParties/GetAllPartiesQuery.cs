using MediatR;
using PartyService.Application.DTOs;

namespace PartyService.Application.Queries.GetAllParties;

public record GetAllPartiesQuery : IRequest<IEnumerable<PartyDto>>;