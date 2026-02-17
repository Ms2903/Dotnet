using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IRatingRepository
{
    Task AddRatingAsync(Rating rating);
    Task<IEnumerable<Rating>> GetRatingsByTargetAsync(Guid targetId);
    Task<IEnumerable<Rating>> GetRatingsByAuthorAsync(Guid authorId);
}
