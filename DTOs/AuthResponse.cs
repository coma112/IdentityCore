using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt,
        string Username,
        string Email
    );
}
