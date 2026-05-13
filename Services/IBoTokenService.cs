using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface IBoTokenService
    {
        string GenerateAccessToken(BoUser boUser);
        Task<BoRefreshToken> GenerateRefreshTokenAsync(BoUser boUser);
        Task<(string accessToken, BoRefreshToken refreshToken, BoUser boUser)> RotateRefreshTokenAsync(string oldToken);
        Task RevokeRefreshTokenAsync(string token, string boUserId);
        Task RevokeAllBoUserTokensAsync(string boUserId);
    }
}