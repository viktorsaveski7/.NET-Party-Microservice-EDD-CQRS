using PartyService.Domain.Entities;

namespace PartyService.Application.Interfaces;

public interface IPartyRepository
{
    Task<Party> CreateAsync(Party party);
    Task<Party?> GetByIdAsync(Guid id);
    Task<IEnumerable<Party>> GetAllAsync();
    Task<Party> UpdateAsync(Party party);
    Task<bool> DeleteAsync(Guid id);
}