using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface ICourtService
{
    Task<CourtResponseDto> CreateCourtAsync(CreateCourtRequestDto request, Guid userId);
    Task<CourtResponseDto> UpdateCourtAsync(Guid courtId, UpdateCourtRequestDto request, Guid userId);
    Task DeleteCourtAsync(Guid courtId, Guid userId);
    Task<IEnumerable<CourtResponseDto>> GetCourtsByVenueIdAsync(Guid venueId);
    Task<CourtResponseDto> GetCourtByIdAsync(Guid courtId);
}
