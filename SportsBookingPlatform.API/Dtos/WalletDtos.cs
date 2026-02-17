using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class AddFundsRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string ReferenceId { get; set; } = string.Empty; // e.g. Payment Gateway ID
}

public class WalletResponseDto
{
    public Guid WalletId { get; set; }
    public decimal Balance { get; set; }
    public Guid UserId { get; set; }
}

public class WalletTransactionResponseDto
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Credit/Debit
    public string ReferenceId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
