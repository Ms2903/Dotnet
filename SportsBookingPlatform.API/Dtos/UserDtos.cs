using System.Text.Json;

namespace SportsBookingPlatform.API.Dtos;

public class UserProfileResponseDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public List<string> PreferredSports { get; set; } = new List<string>();
}
