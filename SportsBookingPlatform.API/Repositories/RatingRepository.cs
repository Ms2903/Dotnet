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
}
