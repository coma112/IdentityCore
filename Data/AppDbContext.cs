using IdentityCore.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityCore.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<Player>(options)
    {
        public DbSet<RefreshToken> RefresthTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Player>(e =>
            {
                e.ToTable("Players");
            });

            builder.Entity<RefreshToken>(e =>
            {
                e.ToTable("RefreshTokens");
                e.HasIndex(t => t.Token).IsUnique();
                e.HasOne(t => t.Player)
                 .WithMany()
                 .HasForeignKey(t => t.PlayerId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
