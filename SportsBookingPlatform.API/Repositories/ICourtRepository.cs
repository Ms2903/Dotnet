using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface ICourtRepository
{
    Task<Court> CreateCourtAsync(Court court);
    Task<Court?> GetCourtByIdAsync(Guid id);
    Task<IEnumerable<Court>> GetCourtsByVenueIdAsync(Guid venueId);
    Task UpdateCourtAsync(Court court);
    Task DeleteCourtAsync(Court court);
    Task<bool> HasFutureBookingsAsync(Guid courtId);
}
