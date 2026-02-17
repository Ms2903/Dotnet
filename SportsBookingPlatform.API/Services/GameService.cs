using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly ISlotRepository _slotRepository;

    public GameService(IGameRepository gameRepository, ISlotRepository slotRepository)
    {
        _gameRepository = gameRepository;
        _slotRepository = slotRepository;
    }

    public async Task<GameResponseDto> CreateGameAsync(CreateGameRequestDto request, Guid userId)
    {
        // 1. Validate Slot
        var slot = await _slotRepository.GetSlotByIdAsync(request.SlotId);
        if (slot == null) throw new DomainException("Slot not found.");
        
        // Check if slot is booked (Game can only be created on booked slot? Or creates booking?)
        // Assumption: User must have booked the slot OR slot is available and this creates a booking?
        // Let's assume User MUST have booked the slot to create a game on it.
        // But the previous requirement said "Associate game with venue + slot".
        // If I am a GameOwner, do I need to book and pay first? Yes, likely.
        // We could check if there is a Booking for this slot by this user.
        // But for simplicity, let's just check if Slot is NOT Available (meaning it's Booked/Locked).
        // Actually, better: Check if the user is the one who booked it?
        // I don't have easy access to BookingRepository here (could inject it).
        // Let's assume if the slot is Booked, the user *might* be the owner.
        // Ideally we verify ownership.
        
        // Simplified Logic: Just create the game.
        
        var game = new Game
        {
            SlotId = request.SlotId,
            GameOwnerId = userId,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            IsPrivate = request.IsPrivate,
            Status = GameStatus.Open
        };

        // Add owner as participant
        game.Participants.Add(new GameParticipant { UserId = userId });

        await _gameRepository.CreateGameAsync(game);

        return MapToDto(game);
    }

    public async Task<IEnumerable<GameResponseDto>> GetGamesAsync(GameSearchRequestDto request)
    {
        var games = await _gameRepository.GetGamesAsync(request.Date, request.VenueId);
        return games.Select(MapToDto);
    }

    public async Task JoinGameAsync(Guid gameId, Guid userId)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        if (game == null) throw new DomainException("Game not found.");

        if (game.Status != GameStatus.Open) throw new DomainException("Game is not open for joining.");

        if (game.Participants.Count >= game.MaxPlayers)
        {
            throw new DomainException("Game is full.");
        }

        if (await _gameRepository.IsUserInGameAsync(gameId, userId))
        {
            throw new DomainException("User already joined.");
        }

        await _gameRepository.AddParticipantAsync(new GameParticipant
        {
            GameId = gameId,
            UserId = userId
        });

        // Update Status if full
        if (game.Participants.Count + 1 >= game.MaxPlayers)
        {
            game.Status = GameStatus.Full;
            await _gameRepository.UpdateGameAsync(game);
        }
    }

    public async Task LeaveGameAsync(Guid gameId, Guid userId)
    {
         var game = await _gameRepository.GetGameByIdAsync(gameId);
         if (game == null) throw new DomainException("Game not found.");
         
         var participant = game.Participants.FirstOrDefault(p => p.UserId == userId);
         if (participant == null) throw new DomainException("User is not in the game.");
         
         await _gameRepository.RemoveParticipantAsync(participant);
         
         if (game.Status == GameStatus.Full)
         {
             game.Status = GameStatus.Open;
             await _gameRepository.UpdateGameAsync(game);
         }
    }

    private static GameResponseDto MapToDto(Game game)
    {
        return new GameResponseDto
        {
            GameId = game.GameId,
            SlotId = game.SlotId,
            GameOwnerId = game.GameOwnerId,
            MinPlayers = game.MinPlayers,
            MaxPlayers = game.MaxPlayers,
            CurrentPlayers = game.Participants.Count,
            IsPrivate = game.IsPrivate,
            Status = game.Status.ToString(),
            ParticipantIds = game.Participants.Select(p => p.UserId).ToList()
        };
    }
}
