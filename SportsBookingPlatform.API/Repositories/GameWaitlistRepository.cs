using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class GameWaitlistRepository : IGameWaitlistRepository
{
    private readonly AppDbContext _context;

    public GameWaitlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddToWaitlistAsync(GameWaitlist waitlistEntry)
    {
        _context.GameWaitlists.Add(waitlistEntry);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromWaitlistAsync(GameWaitlist waitlistEntry)
    {
        _context.GameWaitlists.Remove(waitlistEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<GameWaitlist?> GetWaitlistEntryAsync(Guid gameId, Guid userId)
    {
        return await _context.GameWaitlists
            .FirstOrDefaultAsync(w => w.GameId == gameId && w.UserId == userId);
    }

    public async Task<IEnumerable<GameWaitlist>> GetWaitlistForGameAsync(Guid gameId)
    {
        return await _context.GameWaitlists
            .Where(w => w.GameId == gameId)
            .OrderBy(w => w.JoinedAt)
            .ToListAsync();
    }

    public async Task<int> GetWaitlistCountAsync(Guid gameId)
    {
        return await _context.GameWaitlists.CountAsync(w => w.GameId == gameId);
    }

    public async Task<GameWaitlist?> GetNextInLineAsync(Guid gameId)
    {
        return await _context.GameWaitlists
            .Where(w => w.GameId == gameId && !w.IsNotified)
            .OrderBy(w => w.JoinedAt)
            .FirstOrDefaultAsync();
    }
}
