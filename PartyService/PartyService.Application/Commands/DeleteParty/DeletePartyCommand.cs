using MediatR;

namespace PartyService.Application.Commands.DeleteParty;

public record DeletePartyCommand(Guid Id) : IRequest<bool>;