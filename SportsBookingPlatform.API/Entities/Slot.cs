using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum SlotStatus
{
    Available,
    Locked,
    Booked
}

public class Slot
{
    [Key]
    public Guid SlotId { get; set; } = Guid.NewGuid();

    public Guid CourtId { get; set; }
    public Court? Court { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    public decimal BasePrice { get; set; } // Snapshot of base price at creation time? Or dynamic? Using court base price usually, but maybe good to store here if it varies.
    
    // For now, let's just stick to the definition. 
    // "BasePrice" is on Court, but Slot might have a specific override? 
    // The requirements say "BasePrice" is on Court. 
    // Let's keep it simple.
}
