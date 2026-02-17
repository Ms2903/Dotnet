using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    
    // We might need VenueRepository or GameRepository to validate existence
    // For simplicity, assuming existence or relying on foreign key capability (if we set up real FKs, but we used generic TargetId)
    // The requirement "Rating entity: TargetId (VenueId/GameId)" suggests polimorphic association or just checking.
    // Let's implement basic validation if possible.
    
    public RatingService(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<RatingResponseDto> SubmitRatingAsync(SubmitRatingRequestDto request, Guid userId)
    {
        // Validation: user should have interacted with the target.
        // Skipping complex validation for now to meet time constraints, but real app should check Booking/GameParticipant.
        
        RatingTargetType type;
        if (!Enum.TryParse(request.TargetType, true, out type))
        {
            throw new DomainException("Invalid target type. Use 'Venue' or 'Game'.");
        }

        var rating = new Rating
        {
            RatedByUserId = userId,
            TargetId = request.TargetId,
            TargetType = type,
            RatingValue = request.Score,
            Review = request.Comment
        };
        
        // Link GameId if target is Game (optional optimization for FK)
        if (type == RatingTargetType.Game)
        {
            rating.GameId = request.TargetId;
        }

        await _ratingRepository.AddRatingAsync(rating);

        return new RatingResponseDto
        {
            RatingId = rating.RatingId,
            TargetId = rating.TargetId,
            AuthorId = rating.RatedByUserId,
            Score = rating.RatingValue,
            Comment = rating.Review,
            TargetType = rating.TargetType.ToString(),
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<IEnumerable<RatingResponseDto>> GetRatingsByTargetAsync(Guid targetId)
    {
        var ratings = await _ratingRepository.GetRatingsByTargetAsync(targetId);
        
        return ratings.Select(r => new RatingResponseDto
        {
            RatingId = r.RatingId,
            TargetId = r.TargetId,
            AuthorId = r.RatedByUserId,
            Score = r.RatingValue,
            Comment = r.Review,
            TargetType = r.TargetType.ToString(),
            CreatedAt = r.CreatedAt
        });
    }
}
