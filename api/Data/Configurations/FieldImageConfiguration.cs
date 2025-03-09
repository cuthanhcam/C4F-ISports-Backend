using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
    {
        public void Configure(EntityTypeBuilder<FieldImage> builder)
        {
            builder.HasKey(fi => fi.FieldImageId);
            builder.HasOne(fi => fi.Field)
                   .WithMany()
                   .HasForeignKey(fi => fi.FieldId);
            builder.Property(fi => fi.ImageUrl).IsRequired();
            builder.Property(fi => fi.Thumbnail).HasMaxLength(255);
        }
    }
}