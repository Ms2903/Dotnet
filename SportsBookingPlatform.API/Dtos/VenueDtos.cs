using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class CreateVenueRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public List<string> SportsSupported { get; set; } = new();
}

public class VendorVenueResponseDto
{
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string> SportsSupported { get; set; } = new();
    public string ApprovalStatus { get; set; } = string.Empty;
}
