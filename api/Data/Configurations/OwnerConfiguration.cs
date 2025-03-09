using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
    {
        public void Configure(EntityTypeBuilder<Owner> builder)
        {
            builder.HasKey(o => o.OwnerId);
            builder.HasOne(o => o.Account)
                   .WithMany()
                   .HasForeignKey(o => o.AccountId);
            builder.Property(o => o.FullName).HasMaxLength(100).IsRequired();
            builder.Property(o => o.Phone).HasMaxLength(20).IsRequired();
            builder.Property(o => o.Email).HasMaxLength(255).IsRequired();
        }
    }
}