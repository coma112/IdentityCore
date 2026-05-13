using IdentityCore.Data;
using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Enums;
using IdentityCore.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityCore.Services
{
    public class BoUserService : IBoUserService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<BoUser> _boUserManager;
        private readonly IWalletService _walletService;

        public BoUserService(
            AppDbContext db,
            UserManager<BoUser> boUserManager,
            IWalletService walletService)
        {
            _db = db;
            _boUserManager = boUserManager;
            _walletService = walletService;
        }

        public async Task<BoUser> InviteBoUserAsync(InviteBoUserRequest request, string invitedByBoUserId)
        {
            var boUser = new BoUser
            {
                UserName = request.Username,
                Email = request.Email,
                Role = request.Role,
                EmailConfirmed = true,
                InvitedById = invitedByBoUserId
            };

            var result = await _boUserManager.CreateAsync(boUser, request.Password);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Failed to create BO user.", result.Errors.Select(e => e.Description));
            }

            _db.AuditLogs.Add(new AuditLog
            {
                Action = AuditAction.BoUserInvited,
                BoUserId = invitedByBoUserId,
                TargetBoUserId = boUser.Id,
                Notes = $"Role: {request.Role}"
            });

            await _db.SaveChangesAsync();
            return boUser;
        }

        public async Task GiveBonusAsync(string boUserId, string playerId, decimal amount, string? notes)
        {
            await _walletService.DepositAsync(playerId, amount);

            _db.AuditLogs.Add(new AuditLog
            {
                Action = AuditAction.BonusGiven,
                BoUserId = boUserId,
                TargetPlayerId = playerId,
                Amount = amount,
                Notes = notes,
            });

            await _db.SaveChangesAsync();
        }

        public async Task<PagedResponse<PlayerSummaryResponse>> GetPlayersAsync(int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();
            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PlayerSummaryResponse(p))
                .ToListAsync();

            return new PagedResponse<PlayerSummaryResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
            };
        }

        public async Task<WalletResponse> GetPlayerBalanceAsync(string playerId)
        {
            var player = await _db.Users.FindAsync(playerId)
                ?? throw new NotFoundException("Player not found.");

            var wallet = await _walletService.GetWalletAsync(player.Id);
            return new WalletResponse(wallet);
        }

        public async Task<PagedResponse<TransactionResponse>> GetPlayerTransactionsAsync(
            string playerId, TransactionHistoryRequest request)
        {
            _ = await _db.Users.FindAsync(playerId)
                ?? throw new NotFoundException("Player not found.");

            return await _walletService.GetTransactionHistoryAsync(playerId, request);
        }

        public async Task<List<BoUserResponse>> GetTeamAsync()
        {
            return await _db.BoUsers
                .Include(u => u.InvitedBy)
                .OrderBy(u => u.CreatedAt)
                .Select(u => new BoUserResponse(u))
                .ToListAsync();
        }

        public async Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize)
        {
            var query = _db.AuditLogs
                .Include(a => a.BoUser)
                .Include(a => a.TargetPlayer)
                .Include(a => a.TargetBoUser)
                .AsQueryable();

            int total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogResponse(a))
                .ToListAsync();

            return new PagedResponse<AuditLogResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
            };
        }
    }
}
