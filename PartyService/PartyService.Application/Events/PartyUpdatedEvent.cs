namespace PartyService.Application.Events;

public class PartyUpdatedEvent
{
    public Guid PartyId { get; set; }
    public string BirthdayChildName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? BirthdayChildPhotoUrl { get; set; }
    public DateTime OccurredAt { get; set; }
}