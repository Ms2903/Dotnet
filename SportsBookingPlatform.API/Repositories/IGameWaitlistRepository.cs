using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Repositories;

public interface IGameWaitlistRepository
{
    Task AddToWaitlistAsync(GameWaitlist waitlistEntry);
    Task RemoveFromWaitlistAsync(GameWaitlist waitlistEntry);
    Task<GameWaitlist?> GetWaitlistEntryAsync(Guid gameId, Guid userId);
    Task<IEnumerable<GameWaitlist>> GetWaitlistForGameAsync(Guid gameId);
    Task<int> GetWaitlistCountAsync(Guid gameId);
    Task<GameWaitlist?> GetNextInLineAsync(Guid gameId);
}
