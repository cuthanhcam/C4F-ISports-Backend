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
            builder.Property(a => a.RefreshToken)
                   .HasMaxLength(255);

            // Quan hệ 1-1 với User và Owner
            builder.HasOne(a => a.User)
                   .WithOne(u => u.Account)
                   .HasForeignKey<User>(u => u.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Owner)
                   .WithOne(o => o.Account)
                   .HasForeignKey<Owner>(o => o.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}