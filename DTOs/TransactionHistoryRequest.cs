using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class TransactionHistoryRequest
    {
        /// <summary>
        /// Filter by type: Deposit or Withdrawal, Null = all
        /// </summary>
        public string? Type { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}
