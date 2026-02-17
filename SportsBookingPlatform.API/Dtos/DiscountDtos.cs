using System.ComponentModel.DataAnnotations;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Dtos;

public class CreateDiscountRequestDto
{
    [Required]
    public Guid VenueId { get; set; }

    [Required]
    public DiscountScope Scope { get; set; }

    public Guid? CourtId { get; set; } // Optional if Scope is Venue

    [Required]
    [Range(0.01, 100)]
    public decimal PercentOff { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }
}

public class DiscountResponseDto
{
    public Guid DiscountId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public Guid VenueId { get; set; }
    public Guid? CourtId { get; set; }
    public decimal PercentOff { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}
