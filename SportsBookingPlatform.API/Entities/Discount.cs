using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum DiscountScope
{
    Venue,
    Court
}

public class Discount
{
    [Key]
    public Guid DiscountId { get; set; } = Guid.NewGuid();

    public DiscountScope Scope { get; set; }

    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }

    public Guid? CourtId { get; set; }
    public Court? Court { get; set; }

    public decimal PercentOff { get; set; } // e.g. 10.5 for 10.5%

    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;
}
