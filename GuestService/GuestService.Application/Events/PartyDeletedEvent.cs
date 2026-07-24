namespace GuestService.Application.Events;

public class PartyDeletedEvent
{
    public Guid PartyId { get; set; }
    public DateTime OccurredAt { get; set; }
}