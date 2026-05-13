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
        public DbSet<BoUser> BoUsers => Set<BoUser>();
        public DbSet<BoRefreshToken> BoRefreshTokens => Set<BoRefreshToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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
                e.Property(w => w.Balance).HasPrecision(18, 4);
                e.Property(w => w.Currency).HasMaxLength(3).IsRequired();
                e.HasOne(w => w.Player)
                    .WithMany()
                    .HasForeignKey(w => w.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<WalletTransaction>(e =>
            {
                e.ToTable("WalletTransactions");
                e.Property(t => t.Amount).HasPrecision(18, 4);
                e.Property(t => t.BalanceAfter).HasPrecision(18, 4);
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

            // BO

            builder.Entity<BoUser>(e =>
            {
                e.ToTable("BoUsers");
                e.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                e.HasOne(u => u.InvitedBy)
                    .WithMany()
                    .HasForeignKey(u => u.InvitedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<BoRefreshToken>(e =>
            {
                e.ToTable("BoRefreshTokens");
                e.HasIndex(t => t.Token).IsUnique();
                e.HasOne(t => t.BoUser)
                    .WithMany()
                    .HasForeignKey(t => t.BoUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AuditLog>(e =>
            {
                e.ToTable("AuditLogs");
                e.Property(a => a.Action)
                    .HasConversion<string>()
                    .HasMaxLength(30);
                e.Property(a => a.Amount).HasPrecision(18, 4);

                e.HasOne(a => a.BoUser)
                    .WithMany()
                    .HasForeignKey(a => a.BoUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.TargetPlayer)
                    .WithMany()
                    .HasForeignKey(a => a.TargetPlayerId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(a => a.TargetBoUser)
                    .WithMany()
                    .HasForeignKey(a => a.TargetBoUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(a => a.BoUserId);
                e.HasIndex(a => a.CreatedAt);
            });
        }
    }
}