namespace GuestService.Domain.Entities;

public class CachedParty
{
    public Guid Id { get; set; }
    public string BirthdayChildName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? BirthdayChildPhotoUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation property
    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
}