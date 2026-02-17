using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface ISlotService
{
    Task GenerateSlotsAsync(GenerateSlotsRequestDto request);
    Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(SlotSearchRequestDto request);
}
