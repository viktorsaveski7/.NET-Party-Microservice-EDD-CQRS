using GuestService.Application.DTOs;
using GuestService.Domain.Entities;

namespace GuestService.Application.Mappers;

public static class GuestMapper
{
    public static GuestDto ToDto(Guest guest)
    {
        return new GuestDto(
            guest.Id,
            guest.PartyId,
            guest.FullName,
            guest.Age,
            guest.CreatedAt
        );
    }
}