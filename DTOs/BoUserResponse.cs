using IdentityCore.Entities;

namespace IdentityCore.DTOs
{
    public class BoUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? InvitedByUsername { get; set; }

        public BoUserResponse(BoUser u)
        {
            Id = u.Id;
            Username = u.UserName!;
            Email = u.Email!;
            Role = u.Role.ToString();
            CreatedAt = u.CreatedAt;
            InvitedByUsername = u.InvitedBy?.UserName;
        }
    }
}
