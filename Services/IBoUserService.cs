using IdentityCore.DTOs;
using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface IBoUserService
    {
        Task<BoUser> InviteBoUserAsync(InviteBoUserRequest request, string invitedByBoUserId);
        Task GiveBonusAsync(string boUserId, string playerId, decimal amount, string? notes);
        Task<PagedResponse<PlayerSummaryResponse>> GetPlayersAsync(int page, int pageSize);
        Task<WalletResponse> GetPlayerBalanceAsync(string playerId);
        Task<PagedResponse<TransactionResponse>> GetPlayerTransactionsAsync(string playerId, TransactionHistoryRequest request);
        Task<List<BoUserResponse>> GetTeamAsync();
        Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize);
    }
}
