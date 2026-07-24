namespace GuestService.Application.DTOs;

public record GuestDto(
    Guid Id,
    Guid PartyId,
    string FullName,
    int? Age,
    DateTime CreatedAt
);