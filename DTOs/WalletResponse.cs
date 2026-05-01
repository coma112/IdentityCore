namespace IdentityCore.DTOs
{
    public class WalletResponse
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public WalletResponse(Entities.Wallet wallet)
        {
            Id = wallet.Id;
            Balance = wallet.Balance;
            Currency = wallet.Currency;
            CreatedAt = wallet.CreatedAt;
        }
    }
}
