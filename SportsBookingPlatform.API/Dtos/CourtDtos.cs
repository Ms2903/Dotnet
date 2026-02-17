using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class CreateCourtRequestDto
{
    [Required]
    public Guid VenueId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SportType { get; set; } = string.Empty;

    public int SlotDurationMinutes { get; set; } = 60;

    public decimal BasePrice { get; set; }

    public List<OperatingHourDto> OperatingHours { get; set; } = new();
}

public class UpdateCourtRequestDto
{
    public string? Name { get; set; }
    public decimal? BasePrice { get; set; }
    public bool? IsActive { get; set; }
    public int? SlotDurationMinutes { get; set; }
}

public class OperatingHourDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
}

public class CourtResponseDto
{
    public Guid CourtId { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<OperatingHourDto> OperatingHours { get; set; } = new();
}
