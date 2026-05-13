using IdentityCore.Entities;

namespace IdentityCore.DTOs
{
    public class AuditLogResponse
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string BoUsername { get; set; } = string.Empty;
        public string? TargetPlayerUsername { get; set; }
        public string? TargetBoUsername { get; set; }
        public decimal? Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public AuditLogResponse(AuditLog log)
        {
            Id = log.Id;
            Action = log.Action.ToString();
            BoUsername = log.BoUser?.UserName ?? log.BoUserId;
            TargetPlayerUsername = log.TargetPlayer?.UserName;
            TargetBoUsername = log.TargetBoUser?.UserName;
            Amount = log.Amount;
            Notes = log.Notes;
            CreatedAt = log.CreatedAt;
        }
    }
}
