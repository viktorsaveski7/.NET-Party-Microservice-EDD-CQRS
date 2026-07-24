using GuestService.Application.Interfaces;
using GuestService.Domain.Entities;
using GuestService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuestService.Infrastructure.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly GuestDbContext _context;

    public GuestRepository(GuestDbContext context)
    {
        _context = context;
    }

    public async Task<Guest> CreateAsync(Guest guest)
    {
        // EF Core: Add entity to context
        _context.Guests.Add(guest);

        // EF Core: Save changes (generates and executes SQL)
        await _context.SaveChangesAsync();

        return guest;
    }

    public async Task<Guest?> GetByIdAsync(Guid id)
    {
        // EF Core: LINQ query (auto-generates SQL)
        return await _context.Guests
            .Include(g => g.Party) // Eagerly load Party navigation
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Guest>> GetAllAsync()
    {
        // EF Core: LINQ to get all
        return await _context.Guests
            .Include(g => g.Party)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Guest> UpdateAsync(Guest guest)
    {
        // EF Core: Update entity (change tracking!)
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();

        return guest;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // EF Core: Find and remove
        var guest = await _context.Guests.FindAsync(id);

        if (guest == null)
            return false;

        _context.Guests.Remove(guest);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task DeleteGuestsByPartyIdAsync(Guid partyId)
    {
        var guests = await _context.Guests
            .Where(g => g.PartyId == partyId)
            .ToListAsync();

        if (guests.Any())
        {
            _context.Guests.RemoveRange(guests);
            await _context.SaveChangesAsync();
        }
    }
}