using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IGameService
{
    Task<GameResponseDto> CreateGameAsync(CreateGameRequestDto request, Guid userId);
    Task<IEnumerable<GameResponseDto>> GetGamesAsync(GameSearchRequestDto request);
    Task JoinGameAsync(Guid gameId, Guid userId);
    Task LeaveGameAsync(Guid gameId, Guid userId);
    Task JoinWaitlistAsync(Guid gameId, Guid userId); // Added
    Task LeaveWaitlistAsync(Guid gameId, Guid userId); // Added
}
