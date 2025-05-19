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
                .IsRequired()
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

            // Relationships
            builder.HasOne(a => a.User)
                .WithOne(u => u.Account)
                .HasForeignKey<User>(u => u.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Owner)
                .WithOne(o => o.Account)
                .HasForeignKey<Owner>(o => o.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.RefreshTokens)
                .WithOne(rt => rt.Account)
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}