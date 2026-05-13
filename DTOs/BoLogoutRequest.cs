using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class BoLogoutRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
