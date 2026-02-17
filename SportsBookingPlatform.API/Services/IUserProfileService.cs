using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Services;

public interface IUserProfileService
{
    Task CreateProfileAsync(Guid userId);
    Task IncrementGamesPlayedAsync(Guid userId);
    Task UpdateAverageRatingAsync(Guid userId, decimal newRating);
    Task<UserProfile?> GetProfileByUserIdAsync(Guid userId);
}
