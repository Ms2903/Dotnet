using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
