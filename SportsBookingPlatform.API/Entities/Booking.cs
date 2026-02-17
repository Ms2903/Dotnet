using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum BookingStatus
{
    Pending,
    Locked,
    Confirmed,
    Cancelled,
    Expired,
    Completed
}

public class Booking
{
    [Key]
    public Guid BookingId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid SlotId { get; set; }
    public Slot? Slot { get; set; }

    public decimal LockedPrice { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime LockExpiryTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
