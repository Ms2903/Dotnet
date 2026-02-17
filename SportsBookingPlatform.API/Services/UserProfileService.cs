using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Services;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;

    public UserProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateProfileAsync(Guid userId)
    {
        // Check if exists
        var exists = await _context.UserProfiles.AnyAsync(p => p.UserId == userId);
        if (exists) return;

        var profile = new UserProfile
        {
            UserId = userId,
            GamesPlayed = 0,
            AverageRating = 0,
            TotalRatingsReceived = 0
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
    }

    public async Task IncrementGamesPlayedAsync(Guid userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) 
        {
            // Auto-create if missing
            await CreateProfileAsync(userId);
            profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        if (profile != null)
        {
            profile.GamesPlayed++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAverageRatingAsync(Guid userId, decimal newRating)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
             await CreateProfileAsync(userId);
             profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        if (profile != null)
        {
            // Rolling average logic
            // Average = ((OldAverage * OldCount) + NewRating) / NewCount
            double currentTotal = profile.AverageRating * profile.TotalRatingsReceived;
            profile.TotalRatingsReceived++;
            profile.AverageRating = (currentTotal + (double)newRating) / profile.TotalRatingsReceived;

            await _context.SaveChangesAsync();
        }
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(Guid userId)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
