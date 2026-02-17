using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IBookingService
{
    Task<LockSlotResponseDto> LockSlotAsync(LockSlotRequestDto request, Guid userId);
    Task<BookingResponseDto> ConfirmBookingAsync(ConfirmBookingRequestDto request, Guid userId);
    Task CancelBookingAsync(Guid bookingId, Guid userId);
    Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId);
}
