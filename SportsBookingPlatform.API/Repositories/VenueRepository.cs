using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venue> CreateVenueAsync(Venue venue)
    {
        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

    public async Task<Venue?> GetVenueByIdAsync(Guid id)
    {
        return await _context.Venues
            .Include(v => v.Courts)
            .FirstOrDefaultAsync(v => v.VenueId == id);
    }

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
    {
        return await _context.Venues.ToListAsync();
    }

    public async Task UpdateVenueAsync(Venue venue)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync();
    }
}
