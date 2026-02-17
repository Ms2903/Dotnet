using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class SlotDto
{
    public Guid SlotId { get; set; }
    public Guid CourtId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; } // The base price for this specific slot
    public decimal FinalPrice { get; set; } // Calculated dynamic price
    public string Status { get; set; } = string.Empty;
}

public class SlotSearchRequestDto
{
    public Guid VenueId { get; set; }
    public Guid? CourtId { get; set; }
    public DateTime Date { get; set; }
    public string? SportType { get; set; }
}

public class GenerateSlotsRequestDto
{
    public Guid CourtId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
