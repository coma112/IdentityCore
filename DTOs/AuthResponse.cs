namespace IdentityCore.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public AuthResponse(string accessToken, string refreshToken, DateTime expiresAt, string username, string email)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            Username = username;
            Email = email;
        }
    }
}