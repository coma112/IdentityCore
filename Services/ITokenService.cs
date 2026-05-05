using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Player player);
        Task<RefreshToken> GenerateRefreshTokenAsync(Player player);

        /// <summary>
        /// Rotates the refresh token and gives a new refresh + access token pair.
        /// The player ID is resolved from the token itself.
        /// </summary>
        Task<(string accessToken, RefreshToken refreshToken, Player player)> RotateRefreshTokenAsync(string oldToken);

        /// <summary>
        /// Revokes the given refresh token but only if
        /// the current player has the ownership.
        /// </summary>
        Task RevokeRefreshTokenAsync(string token, string playerId);

        Task RevokeAllPlayerTokensAsync(string playerId);
    }
}