using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IDiscountService
{
    Task<DiscountResponseDto> CreateDiscountAsync(CreateDiscountRequestDto request, Guid venueId, Guid ownerId);
    Task<IEnumerable<DiscountResponseDto>> GetDiscountsForVenueAsync(Guid venueId, Guid userId);
    Task<decimal> ApplyDiscountsAsync(Guid venueId, Guid? courtId, decimal basePrice, DateTime slotTime);
}
