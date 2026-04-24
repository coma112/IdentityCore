using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Entities
{
    public class Player : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeleteRequestedAt { get; set; }
    }
}
