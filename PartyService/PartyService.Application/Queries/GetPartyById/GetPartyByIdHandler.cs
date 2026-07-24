using MediatR;
using PartyService.Application.DTOs;
using PartyService.Application.Exceptions;
using PartyService.Application.Interfaces;
using PartyService.Application.Mappers;

namespace PartyService.Application.Queries.GetPartyById;

public class GetPartyByIdHandler : IRequestHandler<GetPartyByIdQuery, PartyDto>
{
    private readonly IPartyRepository _partyRepository;

    public GetPartyByIdHandler(IPartyRepository partyRepository)
    {
        _partyRepository = partyRepository;
    }

    public async Task<PartyDto> Handle(GetPartyByIdQuery request, CancellationToken cancellationToken)
    {
        var party = await _partyRepository.GetByIdAsync(request.Id);

        if (party == null)
            throw new NotFoundException("Party", request.Id);

        return PartyMapper.ToDto(party);
    }
}