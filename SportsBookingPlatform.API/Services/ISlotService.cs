using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Services;

public interface ISlotService
{
    Task GenerateSlotsAsync(GenerateSlotsRequestDto request);
    Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(SlotSearchRequestDto request);
    void RecordVenueSearch(Guid venueId);
    Task<decimal> CalculateDynamicPrice(Slot slot);
}
