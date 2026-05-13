using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class BoRefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
