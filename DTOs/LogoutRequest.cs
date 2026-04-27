using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public class LogoutRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}