using IdentityCore.Enums;

namespace IdentityCore.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public AuditAction Action { get; set; }

        /// <summary>
        /// The BO user who performed the action.
        /// </summary>
        public string BoUserId { get; set; } = string.Empty;
        public BoUser? BoUser { get; set; }

        /// <summary>
        /// for bonus actions. Null for BO user invite actions!
        /// </summary>
        public string? TargetPlayerId { get; set; }
        public Player? TargetPlayer { get; set; }

        /// <summary>
        /// for invite actions. Null for bonus actions!
        /// </summary>
        public string? TargetBoUserId { get; set; }
        public BoUser? TargetBoUser { get; set; }

        public decimal? Amount { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
