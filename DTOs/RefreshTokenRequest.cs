using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record RefreshTokenRequest(
        [Required] string RefreshToken
    );
}
