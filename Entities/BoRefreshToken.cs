namespace IdentityCore.Entities
{
    /// <summary>
    /// Basically the same as RefreshToken. It's because I use 2 different pipelines for BO and for the actual players.
    /// </summary>
    public class BoRefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string BoUserId { get; set; } = string.Empty;
        public BoUser? BoUser { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
