using IdentityCore.Data;
using IdentityCore.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IdentityCore.Services
{
    public class TokenService(IConfiguration configuration, AppDbContext db) : ITokenService
    {
        private readonly JwtSettings _jwtSettings = configuration
            .GetRequiredSection("Jwt")
            .Get<JwtSettings>()!;

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

            db.RefreshTokens.Add(token);
            await db.SaveChangesAsync();

            return token;
        }

        public async Task<(string accessToken, RefreshToken refreshToken)> RotateRefreshTokenAsync(string oldToken)
        {
            var existing = await db.RefreshTokens
                .Include(t => t.Player)
                .FirstOrDefaultAsync(t => t.Token == oldToken)
                ?? throw new InvalidOperationException("RT nem talalhato");

            if (!existing.IsActive)
                throw new InvalidOperationException("RT mán' nem aktiv");

            existing.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                PlayerId = existing.PlayerId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            };

            db.RefreshTokens.Add(newRefreshToken);
            await db.SaveChangesAsync();

            var accessToken = GenerateAccessToken(existing.Player);
            return (accessToken, newRefreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (existing is { IsRevoked: false })
            {
                existing.RevokedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        public async Task RevokeAllPlayerTokensAsync(string playerId)
        {
            var tokens = await db.RefreshTokens
                .Where(t => t.PlayerId == playerId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
                token.RevokedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
    }
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
