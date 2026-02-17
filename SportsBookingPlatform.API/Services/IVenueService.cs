using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Services;

public interface IVenueService
{
    Task<VendorVenueResponseDto> CreateVenueAsync(CreateVenueRequestDto request, Guid ownerId);
    Task<IEnumerable<VendorVenueResponseDto>> GetAllVenuesAsync();
    Task ApproveVenueAsync(Guid venueId);
}
