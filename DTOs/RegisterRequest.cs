using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record RegisterReques(
        [Required, EmailAddress] string Email,
        [Required, MinLength(3), MaxLength(50)] string Username,
        [Required, MinLength(8)] string Password
    );
}
