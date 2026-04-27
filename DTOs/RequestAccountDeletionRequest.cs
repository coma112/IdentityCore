using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class RequestAccountDeletionRequest
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}