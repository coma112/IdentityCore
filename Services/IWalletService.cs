using IdentityCore.DTOs;
using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface IWalletService
    {
        Task<Wallet> CreateWalletForPlayerAsync(string playerId);
        Task<Wallet> GetWalletAsync(string playerId);
        Task<WalletTransaction> DepositAsync(string playerId, decimal amount);
        Task<WalletTransaction> WithdrawAsync(string playerId, decimal amount);
        Task<PagedResponse<TransactionResponse>> GetTransactionHistoryAsync(string playerId, TransactionHistoryRequest request);
    }
}
