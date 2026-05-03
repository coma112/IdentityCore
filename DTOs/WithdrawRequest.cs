using IdentityCore.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class WithdrawRequest
    {
        [Required]
        [MinDecimal(0.01, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
