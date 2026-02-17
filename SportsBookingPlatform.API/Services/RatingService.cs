using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUserProfileService _userProfileService;
    private readonly IGameRepository _gameRepository; // Added
    
    public RatingService(IRatingRepository ratingRepository, IUserProfileService userProfileService, IGameRepository gameRepository)
    {
        _ratingRepository = ratingRepository;
        _userProfileService = userProfileService;
        _gameRepository = gameRepository;
    }

    public async Task<RatingResponseDto> SubmitRatingAsync(SubmitRatingRequestDto request, Guid userId)
    {
        RatingTargetType type;
        if (!Enum.TryParse(request.TargetType, true, out type))
        {
            throw new DomainException("Invalid target type. Use 'Venue', 'Court', 'Player', or 'Game'.");
        }

        // 1. Validate Game Context
        var game = await _gameRepository.GetGameByIdAsync(request.GameId);
        if (game == null) throw new DomainException("Game not found.");
        
        // 2. Validate Game Status
        // Status must be Completed.
        // Assuming Completed exists in Enum (added in previous step, but I should verify if I actually updated the file or just planned it. I did update Game.cs).
        if (game.Status != GameStatus.Completed)
        {
            throw new DomainException("Ratings can only be submitted for completed games.");
        }
        
        // 3. Validate User Participation
        if (!await _gameRepository.IsUserInGameAsync(request.GameId, userId))
        {
            throw new DomainException("User did not participate in this game.");
        }

        // 4. Validate Uniqueness (One rating per user per game per entity)
        // Note: For 'Game' rating, TargetId is GameId. 
        if (type == RatingTargetType.Game && request.TargetId != request.GameId)
        {
             // Implicitly set TargetId to GameId if type is Game? 
             // Or fail? Let's treat TargetId as the entity being rated.
        }

        if (await _ratingRepository.HasUserRatedAsync(userId, request.TargetId, request.GameId))
        {
            throw new DomainException("You have already rated this entity for this game.");
        }

        var rating = new Rating
        {
            RatedByUserId = userId,
            TargetId = request.TargetId,
            TargetType = type,
            RatingValue = request.Score,
            Review = request.Comment,
            GameId = request.GameId
        };
        
        await _ratingRepository.AddRatingAsync(rating);

        // 5. Update Aggregates
        if (type == RatingTargetType.Player)
        {
            await _userProfileService.UpdateAverageRatingAsync(request.TargetId, request.Score);
        }
        // TODO: Update Venue/Court aggregates if needed (not requested in User Profile Aggregation task, but good for completeness).
        // Requested: "Venue rating summary", "Court rating summary".
        // This implies we compute on read or pre-calculate. 
        // User Profile has "AverageRating" pre-calculated. 
        // Venue/Court might rely on 'GetRatingsByTargetAsync' and averaging in controller or service read.
        // For now, only Player Profile is pre-calculated.
        
        return new RatingResponseDto
        {
            RatingId = rating.RatingId,
            TargetId = rating.TargetId,
            GameId = rating.GameId ?? Guid.Empty,
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
