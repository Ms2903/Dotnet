using SportsBookingPlatform.API.Dtos;

namespace SportsBookingPlatform.API.Services;

public interface IWalletService
{
    Task<WalletResponseDto> GetWalletByUserIdAsync(Guid userId);
    Task<WalletResponseDto> AddFundsAsync(Guid userId, AddFundsRequestDto request);
    Task<bool> DebitFundsAsync(Guid userId, decimal amount, string referenceId); // Internal use mostly
    Task<IEnumerable<WalletTransactionResponseDto>> GetTransactionsAsync(Guid userId);
}
