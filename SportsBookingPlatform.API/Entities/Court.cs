using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public class Court
{
    [Key]
    public Guid CourtId { get; set; } = Guid.NewGuid();

    public Guid VenueId { get; set; }
    public Venue? Venue { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SportType { get; set; } = string.Empty;

    public int SlotDurationMinutes { get; set; } = 60;

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<OperatingHour> OperatingHours { get; set; } = new List<OperatingHour>();
    public ICollection<Slot> Slots { get; set; } = new List<Slot>();
}

public class OperatingHour
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourtId { get; set; }
    public Court? Court { get; set; }

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
}
