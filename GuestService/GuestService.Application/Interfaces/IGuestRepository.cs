using GuestService.Domain.Entities;

namespace GuestService.Application.Interfaces;

public interface IGuestRepository
{
    Task<Guest> CreateAsync(Guest guest);
    Task<Guest?> GetByIdAsync(Guid id);
    Task<IEnumerable<Guest>> GetAllAsync();
    Task<Guest> UpdateAsync(Guest guest);
    Task<bool> DeleteAsync(Guid id);
    Task DeleteGuestsByPartyIdAsync(Guid partyId);
}