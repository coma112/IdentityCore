using IdentityCore.Configuration;
using IdentityCore.Data;
using IdentityCore.Entities;
using IdentityCore.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityCore.Services
{
    public class BoTokenService : IBoTokenService
    {
        private readonly AppDbContext _db;
        private readonly JwtSettings _jwtSettings;

        public BoTokenService(IConfiguration configuration, AppDbContext db)
        {
            _db = db;
            _jwtSettings = configuration
                .GetRequiredSection("Jwt")
                .Get<JwtSettings>()!;
        }

        public string GenerateAccessToken(BoUser boUser)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, boUser.Id),
                new(JwtRegisteredClaimNames.Email, boUser.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.UniqueName, boUser.UserName!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // BO specific claims
                new("actor", "bo"),
                new(ClaimTypes.Role, boUser.Role.ToString()),
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

        public async Task<BoRefreshToken> GenerateRefreshTokenAsync(BoUser boUser)
        {
            var token = new BoRefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                BoUserId = boUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            };

            _db.BoRefreshTokens.Add(token);
            await _db.SaveChangesAsync();

            return token;
        }

        public async Task<(string accessToken, BoRefreshToken refreshToken, BoUser boUser)> RotateRefreshTokenAsync(string oldToken)
        {
            var existing = await _db.BoRefreshTokens
                .Include(t => t.BoUser)
                .FirstOrDefaultAsync(t => t.Token == oldToken);

            if (existing is null || !existing.IsActive)
                throw new UnauthorizedException("Invalid or inactive refresh token.");

            existing.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = new BoRefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                BoUserId = existing.BoUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            };

            _db.BoRefreshTokens.Add(newRefreshToken);
            await _db.SaveChangesAsync();

            var accessToken = GenerateAccessToken(existing.BoUser!);
            return (accessToken, newRefreshToken, existing.BoUser!);
        }

        public async Task RevokeRefreshTokenAsync(string token, string boUserId)
        {
            var existing = await _db.BoRefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (existing is null || existing.BoUserId != boUserId || existing.IsRevoked)
                return;

            existing.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllBoUserTokensAsync(string boUserId)
        {
            await _db.BoRefreshTokens
                .Where(t => t.BoUserId == boUserId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
        }
    }
}
