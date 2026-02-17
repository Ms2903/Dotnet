using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IDiscountRepository
{
    Task<Discount> AddDiscountAsync(Discount discount);
    Task<IEnumerable<Discount>> GetDiscountsByVenueAsync(Guid venueId);
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync(Guid venueId, Guid? courtId, DateTime date);
    Task<Discount?> GetDiscountByIdAsync(Guid discountId);
}
