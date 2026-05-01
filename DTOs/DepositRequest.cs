using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class DepositRequest
    {
        [Required]
        [Range(10, double.MaxValue, ErrorMessage = "Minimum deposit amount is 10.")]
        public decimal Amount { get; set; }
    }
}
