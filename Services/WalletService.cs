using IdentityCore.Data;
using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Enums;
using IdentityCore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace IdentityCore.Services
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _db;

        public WalletService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Wallet> CreateWalletForPlayerAsync(string playerId)
        {
            Wallet wallet = new Wallet
            {
                PlayerId = playerId,
                Balance = 0,
                Currency = "USD",
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();

            return wallet;
        }

        public async Task<Wallet> GetWalletAsync(string playerId)
        {
            return await _db.Wallets
                .FirstOrDefaultAsync(w => w.PlayerId == playerId)
                ?? throw new NotFoundException("Wallet not found.");
        }

        public async Task<WalletTransaction> DepositAsync(string playerId, decimal amount)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    Wallet wallet = await _db.Wallets
                        .FirstOrDefaultAsync(w => w.PlayerId == playerId)
                        ?? throw new NotFoundException("Wallet not found.");

                    wallet.Balance += amount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    // append event source entry
                    WalletTransaction entry = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = amount,
                        Type = TransactionType.Deposit,
                        BalanceAfter = wallet.Balance,
                        Description = "Deposit",
                    };

                    _db.WalletTransactions.Add(entry);

                    // if RowVersion changed since we read (past), EF throws DbUpdateConcurrencyException!!

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return entry;
                } 
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
                {
                    await transaction.RollbackAsync();
                    // detach so next attemtp re-fetches from db.
                    _db.ChangeTracker.Clear();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Deposit failed due to concurrent updates. Please try again.");
        }

        public async Task<WalletTransaction> WithdrawAsync(string playerId, decimal amount)
        {
            const int maxRetries = 3;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    Wallet wallet = await _db.Wallets
                        .FirstOrDefaultAsync(w => w.PlayerId == playerId)
                        ?? throw new NotFoundException("Wallet not found.");

                    if (wallet.Balance < amount)
                    {
                        throw new BadRequestException("Insufficient funds.");
                    }

                    wallet.Balance -= amount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    WalletTransaction entry = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = -amount,
                        Type = TransactionType.Withdrawal,
                        BalanceAfter = wallet.Balance,
                        Description = "Withdrawal",
                    };

                    _db.WalletTransactions.Add(entry);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return entry;
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
                {
                    await transaction.RollbackAsync();
                    _db.ChangeTracker.Clear();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Withdrawal failed due to concurrent updates. Please try again.");
        }

        public async Task<PagedResponse<TransactionResponse>> GetTransactionHistoryAsync(string playerId, TransactionHistoryRequest request)
        {
            Wallet wallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.PlayerId == playerId)
                ?? throw new NotFoundException("Wallet not found.");

            var query = _db.WalletTransactions
                .Where(t => t.WalletId == wallet.Id);

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                if (!Enum.TryParse<TransactionType>(request.Type, ignoreCase: true, out var parsedType))
                    throw new BadRequestException($"Invalid transaction type '{request.Type}'. Use 'Deposit' or 'Withdrawal'.");

                query = query.Where(t => t.Type == parsedType);
            }

            if (request.From.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= request.To.Value);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TransactionResponse(t))
                .ToListAsync();

            return new PagedResponse<TransactionResponse>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
