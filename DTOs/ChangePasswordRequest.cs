using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record ChangePasswordRequest(
        [Required] string CurrentPassword,
        [Required, MinLength(8)] string NewPassword
    );
}
