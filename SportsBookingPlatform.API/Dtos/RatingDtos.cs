using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class SubmitRatingRequestDto
{
    [Required]
    public Guid TargetId { get; set; } // VenueId or GameId

    [Required]
    [Range(1, 5)]
    public int Score { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public string TargetType { get; set; } = "Venue"; // "Venue" or "Game"
}

public class RatingResponseDto
{
    public Guid RatingId { get; set; }
    public Guid TargetId { get; set; } // VenueId or GameId
    public Guid AuthorId { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
