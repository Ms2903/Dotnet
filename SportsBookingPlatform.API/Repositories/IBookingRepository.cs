using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IBookingRepository
{
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId);
    Task UpdateBookingAsync(Booking booking);
}
