using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum VenueApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public class Venue
{
    [Key]
    public Guid VenueId { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    // Stored as JSONB
    public string SportsSupportedJson { get; set; } = "[]";

    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public VenueApprovalStatus ApprovalStatus { get; set; } = VenueApprovalStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}
