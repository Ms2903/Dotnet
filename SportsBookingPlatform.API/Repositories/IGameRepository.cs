using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IGameRepository
{
    Task<Game> CreateGameAsync(Game game);
    Task<Game?> GetGameByIdAsync(Guid id);
    Task<IEnumerable<Game>> GetGamesAsync(DateTime? date, Guid? venueId);
    Task UpdateGameAsync(Game game);
    Task AddParticipantAsync(GameParticipant participant);
    Task RemoveParticipantAsync(GameParticipant participant);
    Task<bool> IsUserInGameAsync(Guid gameId, Guid userId);
}
