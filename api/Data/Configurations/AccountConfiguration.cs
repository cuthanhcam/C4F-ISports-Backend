using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.AccountId);
            builder.Property(a => a.Email).HasMaxLength(255).IsRequired();
            builder.Property(a => a.Password).IsRequired();
            builder.Property(a => a.Role).HasMaxLength(20).IsRequired();
            builder.Property(a => a.IsActive).IsRequired();
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.LastLogin);
            builder.Property(a => a.RefreshToken);
            builder.Property(a => a.RefreshTokenExpiry);
        }
    }
}