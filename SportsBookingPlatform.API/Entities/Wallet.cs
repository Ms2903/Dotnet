using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum TransactionType
{
    Credit,
    Debit
}

public class Wallet
{
    [Key]
    public Guid WalletId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public decimal Balance { get; set; } = 0;
    
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

public class WalletTransaction
{
    [Key]
    public Guid TransactionId { get; set; } = Guid.NewGuid();

    public Guid WalletId { get; set; }
    public Wallet? Wallet { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public string ReferenceId { get; set; } = string.Empty; // e.g. BookingId or PaymentGatewayId

    public string IdempotencyKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
