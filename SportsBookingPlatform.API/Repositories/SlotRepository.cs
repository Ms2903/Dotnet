using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public class SlotRepository : ISlotRepository
{
    private readonly AppDbContext _context;

    public SlotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddSlotsAsync(IEnumerable<Slot> slots)
    {
        await _context.Slots.AddRangeAsync(slots);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Slot>> GetSlotsByCourtAndDateRangeAsync(Guid courtId, DateTime start, DateTime end)
    {
        return await _context.Slots
            .Where(s => s.CourtId == courtId && s.StartTime >= start && s.EndTime <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<Slot>> GetAvailableSlotsAsync(Guid? venueId, Guid? courtId, DateTime start, DateTime end)
    {
        var query = _context.Slots
            .Include(s => s.Court)
            .Where(s => s.Status == SlotStatus.Available && s.StartTime >= start && s.EndTime <= end);

        if (venueId.HasValue)
        {
            query = query.Where(s => s.Court!.VenueId == venueId.Value);
        }

        if (courtId.HasValue)
        {
            query = query.Where(s => s.CourtId == courtId.Value);
        }
        
        return await query.ToListAsync();
    }

    public async Task<Slot?> GetSlotByIdAsync(Guid slotId)
    {
        return await _context.Slots
            .Include(s => s.Court)
            .FirstOrDefaultAsync(s => s.SlotId == slotId);
    }

    public async Task UpdateSlotAsync(Slot slot)
    {
        _context.Slots.Update(slot);
        await _context.SaveChangesAsync();
    }
}
