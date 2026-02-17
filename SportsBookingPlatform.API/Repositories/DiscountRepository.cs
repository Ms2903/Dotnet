using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly AppDbContext _context;

    public DiscountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Discount> AddDiscountAsync(Discount discount)
    {
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();
        return discount;
    }

    public async Task<IEnumerable<Discount>> GetDiscountsByVenueAsync(Guid venueId)
    {
        return await _context.Discounts
            .Where(d => d.VenueId == venueId)
            .OrderByDescending(d => d.ValidFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync(Guid venueId, Guid? courtId, DateTime date)
    {
        var query = _context.Discounts
            .Where(d => d.IsActive && d.ValidFrom <= date && d.ValidTo >= date && d.VenueId == venueId);

        if (courtId.HasValue)
        {
            // Get Venue-wide OR Specific Court discounts
            query = query.Where(d => d.Scope == DiscountScope.Venue || (d.Scope == DiscountScope.Court && d.CourtId == courtId));
        }
        else
        {
            // Only Venue-wide
            query = query.Where(d => d.Scope == DiscountScope.Venue);
        }

        return await query.ToListAsync();
    }

    public async Task<Discount?> GetDiscountByIdAsync(Guid discountId)
    {
        return await _context.Discounts.FindAsync(discountId);
    }
}
