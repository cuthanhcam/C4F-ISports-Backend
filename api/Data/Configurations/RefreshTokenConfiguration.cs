using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.RefreshTokenId);
            builder.HasOne(rt => rt.Account)
                   .WithMany()
                   .HasForeignKey(rt => rt.AccountId);
            builder.Property(rt => rt.Token).HasMaxLength(255).IsRequired();
            builder.Property(rt => rt.Expires).IsRequired();
            builder.Property(rt => rt.Created).IsRequired();
            builder.Property(rt => rt.Revoked);
            builder.Property(rt => rt.ReplacedByToken);
        }
    }
}