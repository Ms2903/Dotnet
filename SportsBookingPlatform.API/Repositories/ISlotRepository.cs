using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface ISlotRepository
{
    Task AddSlotsAsync(IEnumerable<Slot> slots);
    Task<IEnumerable<Slot>> GetSlotsByCourtAndDateRangeAsync(Guid courtId, DateTime start, DateTime end);
    Task<IEnumerable<Slot>> GetAvailableSlotsAsync(Guid? venueId, Guid? courtId, DateTime start, DateTime end);
    Task<Slot?> GetSlotByIdAsync(Guid slotId);
    Task UpdateSlotAsync(Slot slot);
}
