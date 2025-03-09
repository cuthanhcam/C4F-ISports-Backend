using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldAmenityConfiguration : IEntityTypeConfiguration<FieldAmenity>
    {
        public void Configure(EntityTypeBuilder<FieldAmenity> builder)
        {
            builder.HasKey(fa => fa.FieldAmenityId);
            builder.HasOne(fa => fa.Field)
                   .WithMany()
                   .HasForeignKey(fa => fa.FieldId);
            builder.Property(fa => fa.AmenityName).HasMaxLength(100).IsRequired();
        }
    }
}