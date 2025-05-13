using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.AccountId)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.Password)
                .HasMaxLength(256);

            builder.Property(a => a.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(a => a.UpdatedAt)
                .IsRequired(false);

            builder.Property(a => a.LastLogin)
                .IsRequired(false);

            builder.Property(a => a.OAuthProvider)
                .HasMaxLength(50);

            builder.Property(a => a.OAuthId)
                .HasMaxLength(100);

            builder.Property(a => a.AccessToken)
                .HasMaxLength(512);

            builder.Property(a => a.VerificationToken)
                .HasMaxLength(256);

            builder.Property(a => a.VerificationTokenExpiry)
                .IsRequired(false);

            builder.Property(a => a.ResetToken)
                .HasMaxLength(256);

            builder.Property(a => a.ResetTokenExpiry)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(a => a.Email)
                .IsUnique();

            builder.HasIndex(a => a.OAuthId)
                .IsUnique();

            // Relationships
            // Giữ ON DELETE CASCADE cho User và Owner vì đây là mối quan hệ 1:1
            builder.HasOne(a => a.User)
                .WithOne(u => u.Account)
                .HasForeignKey<User>(u => u.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Owner)
                .WithOne(o => o.Account)
                .HasForeignKey<Owner>(o => o.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sử dụng ON DELETE NO ACTION cho RefreshTokens để tránh lan truyền xóa
            builder.HasMany(a => a.RefreshTokens)
                .WithOne(rt => rt.Account)
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}