using System.ComponentModel.DataAnnotations;

namespace IdentityCore.DTOs
{
    public record RequestAccountDeletionRequest(
        [Required] string Password
    );
}
