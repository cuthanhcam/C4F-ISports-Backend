using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.HasKey(p => p.PromotionId);
            builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(p => p.Code).IsUnique();
            builder.Property(p => p.Description).HasMaxLength(255);
            builder.Property(p => p.DiscountType).IsRequired().HasMaxLength(20);
            builder.Property(p => p.DiscountValue).HasPrecision(10, 2);
            builder.Property(p => p.StartDate).IsRequired();
            builder.Property(p => p.EndDate).IsRequired();
            builder.Property(p => p.MinBookingValue).HasPrecision(10, 2);
            builder.Property(p => p.MaxDiscountAmount).HasPrecision(10, 2);
            builder.Property(p => p.IsActive).IsRequired();
        }
    }
}