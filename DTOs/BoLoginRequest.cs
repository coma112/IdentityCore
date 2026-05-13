using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class BoLoginRequest
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
