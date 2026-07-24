using MediatR;
using GuestService.Application.DTOs;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;
using GuestService.Application.Mappers;

namespace GuestService.Application.Queries.GetGuestById;

public class GetGuestByIdHandler : IRequestHandler<GetGuestByIdQuery, GuestDto>
{
    private readonly IGuestRepository _guestRepository;

    public GetGuestByIdHandler(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    public async Task<GuestDto> Handle(GetGuestByIdQuery request, CancellationToken cancellationToken)
    {
        var guest = await _guestRepository.GetByIdAsync(request.Id);

        if (guest == null)
            throw new NotFoundException("Guest", request.Id);

        return GuestMapper.ToDto(guest);
    }
}