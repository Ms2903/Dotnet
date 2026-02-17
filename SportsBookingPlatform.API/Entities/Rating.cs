using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum RatingTargetType
{
    Venue,
    Court,
    Player,
    Game
}

public class Rating
{
    [Key]
    public Guid RatingId { get; set; } = Guid.NewGuid();

    public Guid? GameId { get; set; }
    public Game? Game { get; set; }

    public Guid RatedByUserId { get; set; }
    public User? RatedByUser { get; set; }

    public RatingTargetType TargetType { get; set; }

    public Guid TargetId { get; set; } // Could be VenueId, CourtId, or UserId

    [Range(1, 5)]
    public int RatingValue { get; set; }

    public string Review { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
