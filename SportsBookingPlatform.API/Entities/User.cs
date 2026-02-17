using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SportsBookingPlatform.API.Entities;

public enum UserRole
{
    Admin,
    VenueOwner,
    User
}

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserProfile? Profile { get; set; }
    public Wallet? Wallet { get; set; }
    public ICollection<Venue> Venues { get; set; } = new List<Venue>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class UserProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }

    public double AverageRating { get; set; }
    public int GamesPlayed { get; set; }
    
    // Stored as JSONB in DB
    public string PreferredSportsJson { get; set; } = "[]"; 
}
