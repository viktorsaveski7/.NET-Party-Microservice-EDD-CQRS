namespace GuestService.Domain.Entities;

public class Guest
{
    public Guid Id { get; set; }
    public Guid PartyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public CachedParty? Party { get; set; }
}