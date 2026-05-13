using IdentityCore.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class GiveBonusRequest
    {
        [Required]
        [MinDecimal(0.01, ErrorMessage = "Bonus amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }
    }
}
