using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.HasKey(p => p.PromotionId);
            builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(255).IsRequired();
            builder.Property(p => p.DiscountType).HasMaxLength(20).IsRequired();
            builder.Property(p => p.DiscountValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(p => p.StartDate).IsRequired();
            builder.Property(p => p.EndDate).IsRequired();
            builder.Property(p => p.MinBookingValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(p => p.MaxDiscountAmount).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(p => p.IsActive).IsRequired();
        }
    }
}