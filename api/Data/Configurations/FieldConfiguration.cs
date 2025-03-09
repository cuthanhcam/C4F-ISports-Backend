using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.HasKey(f => f.FieldId);
            builder.HasOne(f => f.Sport)
                   .WithMany()
                   .HasForeignKey(f => f.SportId);
            builder.HasOne(f => f.Owner)
                   .WithMany()
                   .HasForeignKey(f => f.OwnerId);
            builder.Property(f => f.FieldName).HasMaxLength(100).IsRequired();
            builder.Property(f => f.Phone).HasMaxLength(20).IsRequired();
            builder.Property(f => f.Address).HasMaxLength(255).IsRequired();
            builder.Property(f => f.OpenHours).HasMaxLength(100).IsRequired();
            builder.Property(f => f.Status).HasMaxLength(20).IsRequired();
            builder.Property(f => f.Latitude).HasColumnType("decimal(9,6)").IsRequired();
            builder.Property(f => f.Longitude).HasColumnType("decimal(9,6)").IsRequired();
            builder.Property(f => f.CreatedAt).IsRequired();
            builder.Property(f => f.UpdatedAt).IsRequired();
        }
    }
}