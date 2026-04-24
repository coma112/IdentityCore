using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Player player);
        Task<RefreshToken> GenerateRefreshTokenAsync(Player player);
        Task<(string accessToken, RefreshToken refreshToken)> RotateRefreshTokenAsync(string old);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllPlayerTokensAsync(string playerId);
    }
}
