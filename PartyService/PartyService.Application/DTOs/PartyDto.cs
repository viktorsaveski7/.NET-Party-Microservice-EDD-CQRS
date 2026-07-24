namespace PartyService.Application.DTOs;

public record PartyDto(
    Guid Id,
    string BirthdayChildName,
    string? Title,
    string? BirthdayChildPhotoUrl,
    DateTime CreatedAt
);