using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            builder.HasOne(u => u.Account)
                   .WithMany()
                   .HasForeignKey(u => u.AccountId);
            builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
            builder.Property(u => u.Phone).HasMaxLength(20).IsRequired();
            builder.Property(u => u.Gender).HasMaxLength(10).IsRequired();
            builder.Property(u => u.DateOfBirth).IsRequired();
            builder.Property(u => u.AvatarUrl).HasMaxLength(255);
        }
    }
}