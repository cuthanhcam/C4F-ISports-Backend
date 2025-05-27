using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Property(u => u.Phone)
                .HasMaxLength(20);

            builder.Property(u => u.Gender)
                .HasMaxLength(10);

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(u => u.LoyaltyPoints)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.City)
                .HasMaxLength(100);

            builder.Property(u => u.District)
                .HasMaxLength(100);

            builder.Property(u => u.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(u => u.DeletedAt == null);

            builder.HasIndex(u => u.AccountId);

            // Mối quan hệ
            builder.HasOne(u => u.Account)
                .WithOne(a => a.User)
                .HasForeignKey<User>(u => u.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.FavoriteFields)
                .WithOne(ff => ff.User)
                .HasForeignKey(ff => ff.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.SearchHistories)
                .WithOne(sh => sh.Account)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}