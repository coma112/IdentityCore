using IdentityCore.Entities;
using IdentityCore.Enums;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Data
{
    public static class BoSeeder
    {
        public static async Task SeedSuperAdminAsync(UserManager<BoUser> boUserManager, IConfiguration configuration)
        {
            var seed = configuration.GetSection("BoSeed");
            var email = seed["SuperAdminEmail"] ?? "superadmin@identitycore.bo";
            var username = seed["SuperAdminUsername"] ?? "superadmin";
            var password = seed["SuperAdminPassword"] ?? throw new InvalidOperationException("BoSeed:SuperAdminPassword is not configured.");

            var existing = await boUserManager.FindByEmailAsync(email);

            if (existing is not null) return;

            var boUser = new BoUser
            {
                UserName = username,
                Email = email,
                Role = BoRole.SuperAdmin,
                EmailConfirmed = true,
                InvitedBy = null
            };

            var result = await boUserManager.CreateAsync(boUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed SuperAdmin BO user: {errors}");
            }
        }
    }
}
