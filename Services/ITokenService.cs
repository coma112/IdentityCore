using IdentityCore.Entities;

namespace IdentityCore.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Player player);
        Task<RefreshToken> GenerateRefreshTokenAsync(Player player);

        /// <summary>
        /// Rotates the refresh token and gives a new refresh + access token pair.
        /// The returned <see cref="RefreshToken"/> has its <see cref="RefreshToken.Player"/>
        /// navigation property populated — no additional DB call needed by the caller.
        /// </summary>
        Task<(string accessToken, RefreshToken refreshToken, Player player)> RotateRefreshTokenAsync(string oldToken, string playerId);

        /// <summary>
        /// Revokes the given refresh token but only if
        /// the current player has the ownership.
        /// </summary>
        Task RevokeRefreshTokenAsync(string token, string playerId);

        Task RevokeAllPlayerTokensAsync(string playerId);
    }
}