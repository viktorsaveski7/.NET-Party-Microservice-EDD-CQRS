using PartyService.Application.DTOs;
using PartyService.Domain.Entities;

namespace PartyService.Application.Mappers;

public static class PartyMapper
{
    public static PartyDto ToDto(Party party)
    {
        return new PartyDto(
            party.Id,
            party.BirthdayChildName,
            party.Title,
            party.BirthdayChildPhotoUrl,
            party.CreatedAt
        );
    }
}