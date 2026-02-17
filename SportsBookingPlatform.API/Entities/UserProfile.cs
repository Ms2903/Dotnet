using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SportsBookingPlatform.API.Entities;

public class UserProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // Matched existing Id

    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public User? User { get; set; }

    public int GamesPlayed { get; set; } = 0;

    public double AverageRating { get; set; } = 0; // Matched existing double
    
    public int TotalRatingsReceived { get; set; } = 0; // New
    
    public string PreferredSportsJson { get; set; } = "[]"; // Matched existing
}
