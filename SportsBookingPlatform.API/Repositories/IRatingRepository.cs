using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IRatingRepository
{
    Task AddRatingAsync(Rating rating);
    Task<IEnumerable<Rating>> GetRatingsByTargetAsync(Guid targetId);
    Task<IEnumerable<Rating>> GetRatingsByAuthorAsync(Guid authorId);
    Task<bool> HasUserRatedAsync(Guid userId, Guid targetId, Guid? gameId);
    Task<Dictionary<Guid, double>> GetAverageRatingsByTargetTypeAsync(string targetType);
}
