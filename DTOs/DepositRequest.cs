using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class DepositRequest
    {
        [Required]
        [Range(typeof(decimal), "10", "79228162514264337593543950335",
        ErrorMessage = "Minimum deposit amount is 10.")]
        public decimal Amount { get; set; }
    }
}
