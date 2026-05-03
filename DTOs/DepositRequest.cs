using IdentityCore.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class DepositRequest
    {
        [Required]
        [MinDecimal(10, ErrorMessage = "Minimum deposit amount is 10.")]
        public decimal Amount { get; set; }
    }
}
