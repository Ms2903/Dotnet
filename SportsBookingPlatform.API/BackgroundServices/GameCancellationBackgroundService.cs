using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.BackgroundServices;

public class GameCancellationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameCancellationBackgroundService> _logger;

    public GameCancellationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<GameCancellationBackgroundService> logger)
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
                await ProcessCancellationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling games.");
            }

            // Run every minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessCancellationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // We might need WalletService logic, but easier to use Context directly or inject Service if designed for Scope.
        // WalletService is Scoped, so we can resolve it.
        // But WalletService might rely on HTTP Context or similar? No, just DbContext.
        
        var now = DateTime.UtcNow;
        var threshold = now.AddHours(1);

        // Find games that are Open, starting soon, and haven't met min players
        // We need Slot StartTime.
        var gamesToCancel = await context.Games
            .Include(g => g.Slot)
            .Include(g => g.Participants)
            .Where(g => g.Status == GameStatus.Open 
                        && g.Slot.StartTime <= threshold 
                        && g.Slot.StartTime > now // Don't process already passed games repeatedly if logic fails?
                        && g.Participants.Count < g.MinPlayers)
            .ToListAsync(stoppingToken);

        foreach (var game in gamesToCancel)
        {
            _logger.LogInformation($"Auto-cancelling Game {game.GameId} due to insufficient players.");
            
            // Refund participants
            // Logic similar to CancelBooking? 
            // Here we just refund because it's system cancellation.
            // Assuming Participants paid? 
            // Wait, does JoinGame charge user? 
            // Booking charges VenueOwner/User who booked slot.
            // Game Cancellation usually means the "Game Event" is cancelled.
            // If the GameOwner booked the slot, does the slot get cancelled?
            // "Cancel game, Refund users".
            // If users paid to join (not implemented yet, JoinGame is free in current code), then refund.
            // If only GameOwner paid for Slot, then GameOwner needs refund and Slot might be freed?
            // Requirement says "Refund users".
            // Since JoinGame doesn't charge currently (only Booking does), maybe we mean meaningful refund if we added payments.
            // OR we mean refund the GameOwner if the Game is cancelled?
            // Let's assume GameOwner paid for Slot. 
            // If Game is cancelled, we should Cancel the Booking too?
            // If Slot Booking is separate, maybe we just cancel the Game entity.
            
            // Re-reading requirements: "Cancel game, Refund users".
            // Implies users paid. 
            // But my `JoinGame` implementation didn't charge.
            // However, `BookingService` charges. 
            // If GameOwner booked the slot, we should refund the GameOwner.
            
            // Let's implement logic: 
            // 1. Mark Game as Cancelled.
            // 2. If Slot was booked for this game, should we cancel booking?
            //    Usually yes.
            //    Find booking for this slot?
            var booking = await context.Bookings
                .FirstOrDefaultAsync(b => b.SlotId == game.SlotId && b.UserId == game.GameOwnerId && b.Status == BookingStatus.Confirmed, stoppingToken);
                
            if (booking != null)
            {
                // Refund GameOwner if booking found
                // Using WalletService helper or direct context manipulation if WalletService not easily usable due to its dependency on context (which we have).
                // WalletService constructor takes AppDbContext.
                var walletService = new WalletService(context);
                
                // AddFundsAsync logic:
                await walletService.AddFundsAsync(booking.UserId, new Dtos.AddFundsRequestDto
                {
                    Amount = booking.LockedPrice,
                    ReferenceId = $"AutoRefund:{game.GameId}"
                });
                
                booking.Status = BookingStatus.Cancelled;
                _logger.LogInformation($"Refunded GameOwner {booking.UserId} for Game {game.GameId}");
            }
            
            // Mark Game Cancelled
            game.Status = GameStatus.Cancelled;
            
            // Free up slot 
            if (game.Slot != null) 
            {
                game.Slot.Status = SlotStatus.Available;
            }
        }

        if (gamesToCancel.Any())
        {
            await context.SaveChangesAsync(stoppingToken); // Commit all changes
        }
    }
}
