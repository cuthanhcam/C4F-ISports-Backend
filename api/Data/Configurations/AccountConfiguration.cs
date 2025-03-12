using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.Email)
                   .IsRequired()
                   .HasMaxLength(255);
            builder.HasIndex(a => a.Email)
                   .IsUnique();

            builder.Property(a => a.Password)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(a => a.Role)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(a => a.IsActive)
                   .IsRequired();

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            // Cấu hình ResetToken và ResetTokenExpiry
            builder.Property(a => a.ResetToken)
                   .HasMaxLength(255);

            builder.Property(a => a.ResetTokenExpiry)
                   .IsRequired(false); // Nullable

            // Cấu hình VerificationToken và VerificationTokenExpiry
            builder.Property(a => a.VerificationToken)
                   .HasMaxLength(255);

            builder.Property(a => a.VerificationTokenExpiry)
                   .IsRequired(false); // Nullable

            // Quan hệ 1-1 với User
            builder.HasOne(a => a.User)
                   .WithOne(u => u.Account)
                   .HasForeignKey<User>(u => u.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1-1 với Owner
            builder.HasOne(a => a.Owner)
                   .WithOne(o => o.Account)
                   .HasForeignKey<Owner>(o => o.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1-nhiều với RefreshToken
            builder.HasMany(a => a.RefreshTokens)
                   .WithOne(rt => rt.Account)
                   .HasForeignKey(rt => rt.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}