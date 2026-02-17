using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Slot)
            .ThenInclude(s => s.Court)
            .ThenInclude(c => c.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId)
    {
        return await _context.Bookings
            .Include(b => b.Slot)
            .ThenInclude(s => s.Court)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}
