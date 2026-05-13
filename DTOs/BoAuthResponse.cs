using IdentityCore.Entities;

namespace IdentityCore.DTOs
{
    public class BoAuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public BoAuthResponse(string accessToken, string refreshToken, DateTime expiresAt, BoUser user)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            Username = user.UserName!;
            Email = user.Email!;
            Role = user.Role.ToString();
        }
    }
}
