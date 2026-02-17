using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IVenueRepository _venueRepository;

    public DiscountService(IDiscountRepository discountRepository, IVenueRepository venueRepository)
    {
        _discountRepository = discountRepository;
        _venueRepository = venueRepository;
    }

    public async Task<DiscountResponseDto> CreateDiscountAsync(CreateDiscountRequestDto request, Guid venueId, Guid ownerId)
    {
        // 1. Verify Venue Ownership
        var venue = await _venueRepository.GetVenueByIdAsync(venueId);
        if (venue == null) throw new DomainException("Venue not found.");
        if (venue.OwnerId != ownerId) throw new DomainException("Not authorized to add discount for this venue.");

        // 2. Validate Dates
        if (request.ValidFrom >= request.ValidTo) throw new DomainException("ValidFrom must be before ValidTo.");
        if (request.ValidTo <= DateTime.UtcNow) throw new DomainException("ValidTo must be in the future.");

        // 3. Create Discount
        var discount = new Discount
        {
            Scope = request.Scope,
            VenueId = venueId,
            CourtId = request.Scope == DiscountScope.Court ? request.CourtId : null,
            PercentOff = request.PercentOff,
            ValidFrom = DateTime.SpecifyKind(request.ValidFrom, DateTimeKind.Utc),
            ValidTo = DateTime.SpecifyKind(request.ValidTo, DateTimeKind.Utc),
            IsActive = true
        };

        await _discountRepository.AddDiscountAsync(discount);

        return MapToDto(discount);
    }

    public async Task<IEnumerable<DiscountResponseDto>> GetDiscountsForVenueAsync(Guid venueId, Guid userId)
    {
        // Optional: verify ownership or allow any user to see discounts?
        // Requirement says VenueOwner manages it. Let's assume public can view via pricing, but list is for owner management.
        // Actually, "List discounts" endpoint usually for Owner/Admin.
        var venue = await _venueRepository.GetVenueByIdAsync(venueId);
        if (venue != null && venue.OwnerId != userId)
        {
            // If strict owner check required used here.
            // For now, let's allow it but maybe filter active vs inactive?
        }

        var discounts = await _discountRepository.GetDiscountsByVenueAsync(venueId);
        return discounts.Select(MapToDto);
    }

    public async Task<decimal> ApplyDiscountsAsync(Guid venueId, Guid? courtId, decimal basePrice, DateTime slotTime)
    {
        var discounts = await _discountRepository.GetActiveDiscountsAsync(venueId, courtId, DateTime.SpecifyKind(slotTime, DateTimeKind.Utc));

        // Strategy: Apply max discount? Or stack them?
        // Requirement: "Demand/Time/Historical/Discount factor".
        // Usually, use the highest applicable discount if multiple exist to avoid free slots.
        
        decimal maxPercentOff = 0;
        
        foreach (var d in discounts)
        {
            if (d.PercentOff > maxPercentOff)
            {
                maxPercentOff = d.PercentOff;
            }
        }

        if (maxPercentOff > 0)
        {
            // Factor = (1 - percent/100)
            return basePrice * (1 - (maxPercentOff / 100));
        }

        return basePrice;
    }

    private static DiscountResponseDto MapToDto(Discount discount)
    {
        return new DiscountResponseDto
        {
            DiscountId = discount.DiscountId,
            Scope = discount.Scope.ToString(),
            VenueId = discount.VenueId ?? Guid.Empty, // Should rely on VenueId being set
            CourtId = discount.CourtId,
            PercentOff = discount.PercentOff,
            ValidFrom = discount.ValidFrom,
            ValidTo = discount.ValidTo,
            IsActive = discount.IsActive
        };
    }
}
