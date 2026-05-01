using IdentityCore.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityCore.Data
{
    public class AppDbContext : IdentityDbContext<Player>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

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

            builder.Entity<Wallet>(e =>
            {
                e.ToTable("Wallets");

                e.HasIndex(w => w.PlayerId).IsUnique();

                e.Property(w => w.Balance)
                    .HasPrecision(18, 4);

                e.Property(w => w.Currency)
                    .HasMaxLength(3)
                    .IsRequired();

                e.Property(w => w.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                e.HasOne(w => w.Player)
                    .WithMany()
                    .HasForeignKey(w => w.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<WalletTransaction>(e =>
            {
                e.ToTable("WalletTransactions");

                e.Property(t => t.Amount)
                    .HasPrecision(18, 4);

                e.Property(t => t.BalanceAfter)
                    .HasPrecision(18, 4);

                e.Property(t => t.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                e.HasOne(t => t.Wallet)
                    .WithMany()
                    .HasForeignKey(t => t.WalletId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(t => new { t.WalletId, t.CreatedAt });
                e.HasIndex(t => new { t.WalletId, t.Type });
            });
        }
    }
}