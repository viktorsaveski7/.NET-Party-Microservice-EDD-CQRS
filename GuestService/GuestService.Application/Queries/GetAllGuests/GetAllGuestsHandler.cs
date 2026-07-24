using MediatR;
using GuestService.Application.DTOs;
using GuestService.Application.Interfaces;
using GuestService.Application.Mappers;

namespace GuestService.Application.Queries.GetAllGuests;

public class GetAllGuestsHandler : IRequestHandler<GetAllGuestsQuery, IEnumerable<GuestDto>>
{
    private readonly IGuestRepository _guestRepository;

    public GetAllGuestsHandler(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    public async Task<IEnumerable<GuestDto>> Handle(GetAllGuestsQuery request, CancellationToken cancellationToken)
    {
        var guests = await _guestRepository.GetAllAsync();

        return guests.Select(GuestMapper.ToDto);
    }
}