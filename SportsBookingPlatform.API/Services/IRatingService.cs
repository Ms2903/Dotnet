using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IRatingService
{
    Task<RatingResponseDto> SubmitRatingAsync(SubmitRatingRequestDto request, Guid userId);
    Task<IEnumerable<RatingResponseDto>> GetRatingsByTargetAsync(Guid targetId);
}
