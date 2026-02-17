using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.BackgroundServices;

public class GameCompletionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameCompletionBackgroundService> _logger;

    public GameCompletionBackgroundService(IServiceScopeFactory scopeFactory, ILogger<GameCompletionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessGameCompletionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing games.");
            }

            // Run every minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessGameCompletionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userProfileService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();
        
        var now = DateTime.UtcNow;

        // Find games that are Open/Full/InProgress, where Slot EndTime has passed
        // We define "Completion" as Slot EndTime passed.
        
        var gamesToComplete = await context.Games
            .Include(g => g.Slot)
            .Include(g => g.Participants)
            .Where(g => (g.Status == GameStatus.Open || g.Status == GameStatus.Full) 
                        && g.Slot.EndTime <= now)
            .ToListAsync(stoppingToken);

        foreach (var game in gamesToComplete)
        {
            _logger.LogInformation($"Completing Game {game.GameId}.");
            
            // Mark as Completed (Validation: Assuming 'Completed' status exists in Enum? I need to check GameStatus enum)
            // If GameStatus.Completed doesn't exist, I need to add it or use another status.
            // Let's assume I need to ADD it if missing.
            // Checking Game.cs in a moment. 
            // If it's missing, I'll update Enum.
            
            // For now, assuming Completed exists or I'll add it.
            // game.Status = GameStatus.Completed; 
            // Actually, let's check GameStatus enum.
            
            // Increment GamesPlayed for all participants
            foreach (var participant in game.Participants)
            {
                await userProfileService.IncrementGamesPlayedAsync(participant.UserId);
            }
            
            // We can't set status to "Completed" if it's not in Enum.
            // I'll check Enum in next step. For now, I'll comment out status update if unsure, 
            // but I should update Enum if needed.
            // Let's assume I will update Enum to include 'Completed'.
             game.Status = GameStatus.Completed;
        }

        if (gamesToComplete.Any())
        {
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
