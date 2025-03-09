using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.AccountId).IsRequired();
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Phone).HasMaxLength(20);
            builder.Property(u => u.Gender).HasMaxLength(10);
            builder.Property(u => u.DateOfBirth).IsRequired();
            builder.Property(u => u.AvatarUrl).HasMaxLength(255);
        }
    }
}