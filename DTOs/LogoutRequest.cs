using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record LogoutRequest(
        [Required] string RefreshToken
    );
}
