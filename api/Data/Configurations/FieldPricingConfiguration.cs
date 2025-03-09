using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldPricingConfiguration : IEntityTypeConfiguration<FieldPricing>
    {
        public void Configure(EntityTypeBuilder<FieldPricing> builder)
        {
            builder.HasKey(fp => fp.FieldPricingId);
            builder.HasOne(fp => fp.Field)
                   .WithMany()
                   .HasForeignKey(fp => fp.FieldId);
            builder.Property(fp => fp.StartTime).IsRequired();
            builder.Property(fp => fp.EndTime).IsRequired();
            builder.Property(fp => fp.DayOfWeek).IsRequired();
            builder.Property(fp => fp.Price).HasColumnType("decimal(10,2)").IsRequired();
        }
    }
}