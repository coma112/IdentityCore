using IdentityCore.Enums;

namespace IdentityCore.Entities
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public Wallet? Wallet { get; set; }

        /// <summary>
        /// positive = deposit | negative = withdrawal.
        /// </summary>
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        /// <summary>
        /// balance snapshot after the transaction was applied.
        /// </summary>
        public decimal BalanceAfter { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
