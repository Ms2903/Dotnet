using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;

namespace SportsBookingPlatform.API.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;

    public WalletService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WalletResponseDto> GetWalletByUserIdAsync(Guid userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new DomainException("Wallet not found.");
        
        return new WalletResponseDto
        {
            WalletId = wallet.WalletId,
            Balance = wallet.Balance,
            UserId = wallet.UserId
        };
    }

    public async Task<WalletResponseDto> AddFundsAsync(Guid userId, AddFundsRequestDto request)
    {
        // Idempotency check should be here using ReferenceId on Transactions table
        if (await _context.WalletTransactions.AnyAsync(t => t.ReferenceId == request.ReferenceId && t.Type == TransactionType.Credit))
        {
             // Already processed, return current wallet state
            return await GetWalletByUserIdAsync(userId);
            // Or throw exception depending on requirements. Returning state is idempotent compliant.
        }

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new DomainException("Wallet not found.");

        wallet.Balance += request.Amount;
        
        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = request.Amount,
            Type = TransactionType.Credit,
            ReferenceId = request.ReferenceId,
            IdempotencyKey = Guid.NewGuid().ToString() // Simple generation
        };

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new WalletResponseDto
        {
            WalletId = wallet.WalletId,
            Balance = wallet.Balance,
            UserId = wallet.UserId
        };
    }

    public async Task<bool> DebitFundsAsync(Guid userId, decimal amount, string referenceId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new DomainException("Wallet not found.");

        if (wallet.Balance < amount)
        {
            return false; // Insufficient funds
        }

        wallet.Balance -= amount;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = amount,
            Type = TransactionType.Debit,
            ReferenceId = referenceId,
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        _context.WalletTransactions.Add(transaction);
        // Note: SaveChanges is called by the caller (BookingService) usually within a transaction
        // But here we are injecting logic. If we want this to be atomic with booking, we need to share the context transaction.
        // Since we are using the same injected AppDbContext (Scoped), it shares the transaction if one is active.
        // But we should usually let the caller loop SaveChanges if part of a UoW, or simply await SaveChanges here if independent.
        // For Booking Confirmation, we'll want explicit transaction management. 
        // For now, let's SaveChangesAsync here. If inside a transaction, it won't commit until transaction.Commit().
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<WalletTransactionResponseDto>> GetTransactionsAsync(Guid userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) throw new DomainException("Wallet not found.");

        var transactions = await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.WalletId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(t => new WalletTransactionResponseDto
        {
            TransactionId = t.TransactionId,
            Amount = t.Amount,
            Type = t.Type.ToString(),
            ReferenceId = t.ReferenceId,
            CreatedAt = t.CreatedAt
        });
    }
}
