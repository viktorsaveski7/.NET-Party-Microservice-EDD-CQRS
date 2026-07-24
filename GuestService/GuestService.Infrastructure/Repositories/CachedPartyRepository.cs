using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using GuestService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuestService.Infrastructure.Repositories;

public class CachedPartyRepository : ICachedPartyRepository
{
    private readonly GuestDbContext _context;

    public CachedPartyRepository(GuestDbContext context)
    {
        _context = context;
    }

    public async Task<CachedParty?> GetByIdAsync(Guid id)
    {
        return await _context.CachedParties
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<CachedParty> CreateAsync(CachedParty party)
    {
        _context.CachedParties.Add(party);
        await _context.SaveChangesAsync();
        return party;
    }

    public async Task<CachedParty> UpdateAsync(CachedParty party)
    {
        _context.CachedParties.Update(party);
        await _context.SaveChangesAsync();
        return party;
    }

    public async Task MarkAsDeletedAsync(Guid id)
    {
        var party = await _context.CachedParties.FindAsync(id);

        if (party != null)
        {
            party.IsDeleted = true;
            party.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}