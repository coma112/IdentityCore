using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}