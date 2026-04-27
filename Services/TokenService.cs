using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityCore.Configuration;
using IdentityCore.Data;
using IdentityCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityCore.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _db;
        private readonly JwtSettings _jwtSettings;

        public TokenService(IConfiguration configuration, AppDbContext db)
        {
            _db = db;
            _jwtSettings = configuration
                .GetRequiredSection("Jwt")
                .Get<JwtSettings>()!;
        }

        public string GenerateAccessToken(Player player)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, player.Id),
                new(JwtRegisteredClaimNames.Email, player.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.UniqueName, player.UserName!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(Player player)
        {
            var token = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            };

            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();

            return token;
        }

        public async Task<(string accessToken, RefreshToken refreshToken)> RotateRefreshTokenAsync(string oldToken, string playerId)
        {
            var existing = await _db.RefreshTokens
                .Include(t => t.Player)
                .FirstOrDefaultAsync(t => t.Token == oldToken);

            if (existing is null || existing.PlayerId != playerId || !existing.IsActive)
                throw new InvalidOperationException("Invalid or inactive refresh token.");

            existing.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                PlayerId = existing.PlayerId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            };

            _db.RefreshTokens.Add(newRefreshToken);
            await _db.SaveChangesAsync();

            var accessToken = GenerateAccessToken(existing.Player);
            return (accessToken, newRefreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token, string playerId)
        {
            var existing = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (existing is null || existing.PlayerId != playerId || existing.IsRevoked)
                return;

            existing.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllPlayerTokensAsync(string playerId)
        {
            await _db.RefreshTokens
                .Where(t => t.PlayerId == playerId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
        }
    }
}