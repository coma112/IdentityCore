using IdentityCore.Entities;

namespace IdentityCore.DTOs
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public TransactionResponse(WalletTransaction t)
        {
            Id = t.Id;
            Amount = t.Amount;
            Type = t.Type.ToString();
            BalanceAfter = t.BalanceAfter;
            Description = t.Description;
            CreatedAt = t.CreatedAt;
        }
    }
}
