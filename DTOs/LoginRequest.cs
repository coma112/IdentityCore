using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record LoginRequest(
        [Required] string UsernameOrEmail,
        [Required] string Password
    );
}
