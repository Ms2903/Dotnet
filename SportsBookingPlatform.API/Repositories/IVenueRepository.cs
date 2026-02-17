using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IVenueRepository
{
    Task<Venue> CreateVenueAsync(Venue venue);
    Task<Venue?> GetVenueByIdAsync(Guid id);
    Task<IEnumerable<Venue>> GetAllVenuesAsync();
    Task UpdateVenueAsync(Venue venue);
}
