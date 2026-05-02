using System.ComponentModel.DataAnnotations;

namespace IdentityCore.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public Player Player { get; set; } = null!;
        public decimal Balance { get; set; } = 0;
        public string Currency { get; set; } = "USD";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ConcurrencyCheck]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
