using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Game> CreateGameAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task<Game?> GetGameByIdAsync(Guid id)
    {
        return await _context.Games
            .Include(g => g.Participants)
            .Include(g => g.Slot)
            .FirstOrDefaultAsync(g => g.GameId == id);
    }

    public async Task<IEnumerable<Game>> GetGamesAsync(DateTime? date, Guid? venueId)
    {
        var query = _context.Games
            .Include(g => g.Participants)
            .Include(g => g.Slot)
            .ThenInclude(s => s.Court)
            .AsQueryable();

        if (date.HasValue)
        {
            var start = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);
            query = query.Where(g => g.Slot != null && g.Slot.StartTime >= start && g.Slot.StartTime < end);
        }

        if (venueId.HasValue)
        {
            query = query.Where(g => g.Slot != null && g.Slot.Court != null && g.Slot.Court.VenueId == venueId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task UpdateGameAsync(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task AddParticipantAsync(GameParticipant participant)
    {
        _context.GameParticipants.Add(participant);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveParticipantAsync(GameParticipant participant)
    {
        _context.GameParticipants.Remove(participant);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUserInGameAsync(Guid gameId, Guid userId)
    {
        return await _context.GameParticipants.AnyAsync(gp => gp.GameId == gameId && gp.UserId == userId);
    }
}
