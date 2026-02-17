using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class CourtRepository : ICourtRepository
{
    private readonly AppDbContext _context;

    public CourtRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Court> CreateCourtAsync(Court court)
    {
        _context.Courts.Add(court);
        await _context.SaveChangesAsync();
        return court;
    }

    public async Task<Court?> GetCourtByIdAsync(Guid id)
    {
        return await _context.Courts
            .Include(c => c.OperatingHours)
            .Include(c => c.Venue)
            .FirstOrDefaultAsync(c => c.CourtId == id);
    }

    public async Task<IEnumerable<Court>> GetCourtsByVenueIdAsync(Guid venueId)
    {
        return await _context.Courts
            .Include(c => c.OperatingHours)
            .Where(c => c.VenueId == venueId)
            .ToListAsync();
    }

    public async Task UpdateCourtAsync(Court court)
    {
        _context.Courts.Update(court);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourtAsync(Court court)
    {
        _context.Courts.Remove(court);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasFutureBookingsAsync(Guid courtId)
    {
        return await _context.Bookings
            .AnyAsync(b => b.Slot != null && b.Slot.CourtId == courtId && b.Slot.StartTime > DateTime.UtcNow && b.Status != BookingStatus.Cancelled);
    }
}
