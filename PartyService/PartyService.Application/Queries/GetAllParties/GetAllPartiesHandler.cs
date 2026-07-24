using MediatR;
using PartyService.Application.DTOs;
using PartyService.Application.Interfaces;
using PartyService.Application.Mappers;

namespace PartyService.Application.Queries.GetAllParties;

public class GetAllPartiesHandler : IRequestHandler<GetAllPartiesQuery, IEnumerable<PartyDto>>
{
    private readonly IPartyRepository _partyRepository;

    public GetAllPartiesHandler(IPartyRepository partyRepository)
    {
        _partyRepository = partyRepository;
    }

    public async Task<IEnumerable<PartyDto>> Handle(GetAllPartiesQuery request, CancellationToken cancellationToken)
    {
        var parties = await _partyRepository.GetAllAsync();

        return parties.Select(PartyMapper.ToDto);
    }
}