using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRatingAsync(Rating rating)
    {
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Rating>> GetRatingsByTargetAsync(Guid targetId)
    {
        return await _context.Ratings
            .Where(r => r.TargetId == targetId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rating>> GetRatingsByAuthorAsync(Guid authorId)
    {
        return await _context.Ratings
            .Where(r => r.RatedByUserId == authorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    public async Task<bool> HasUserRatedAsync(Guid userId, Guid targetId, Guid? gameId)
    {
        var query = _context.Ratings
            .Where(r => r.RatedByUserId == userId && r.TargetId == targetId);
            
        if (gameId.HasValue)
        {
            query = query.Where(r => r.GameId == gameId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<Dictionary<Guid, double>> GetAverageRatingsByTargetTypeAsync(string targetType)
    {
        RatingTargetType type;
        if (!Enum.TryParse(targetType, true, out type)) return new Dictionary<Guid, double>();

        return await _context.Ratings
            .Where(r => r.TargetType == type)
            .GroupBy(r => r.TargetId)
            .Select(g => new { TargetId = g.Key, Average = g.Average(r => r.RatingValue) })
            .ToDictionaryAsync(x => x.TargetId, x => x.Average);
    }
}
