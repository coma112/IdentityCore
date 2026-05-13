using IdentityCore.Entities;

namespace IdentityCore.DTOs
{
    public class PlayerSummaryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool DeletionRequested { get; set; }

        public PlayerSummaryResponse(Player p)
        {
            Id = p.Id;
            Username = p.UserName!;
            Email = p.Email!;
            CreatedAt = p.CreatedAt;
            DeletionRequested = p.DeleteRequestedAt.HasValue;
        }
    }
}
