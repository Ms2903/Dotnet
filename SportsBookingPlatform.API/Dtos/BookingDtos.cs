using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class LockSlotRequestDto
{
    [Required]
    public Guid SlotId { get; set; }
}

public class LockSlotResponseDto
{
    public Guid SlotId { get; set; }
    public decimal LockedPrice { get; set; }
    public DateTime ExpiryTime { get; set; }
}

public class ConfirmBookingRequestDto
{
    [Required]
    public Guid SlotId { get; set; }
}

public class BookingResponseDto
{
    public Guid BookingId { get; set; }
    public Guid SlotId { get; set; }
    public Guid UserId { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
