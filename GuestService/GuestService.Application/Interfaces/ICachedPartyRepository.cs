using GuestService.Domain.Entities;

namespace GuestService.Application.Interfaces;

public interface ICachedPartyRepository
{
    Task<CachedParty?> GetByIdAsync(Guid id);
    Task<CachedParty> CreateAsync(CachedParty party);
    Task<CachedParty> UpdateAsync(CachedParty party);
    Task MarkAsDeletedAsync(Guid id);
}