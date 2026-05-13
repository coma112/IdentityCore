using IdentityCore.Enums;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Entities
{
    public class BoUser : IdentityUser
    {
        public BoRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The SuperAdmin BoUser who invited this user. null if the SuperAdmin is the seeded one.
        /// </summary>
        public string? InvitedById { get; set; }
        public BoUser? InvitedBy { get; set; }
    }
}
