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

            builder.Property(p => p.Code)
                .HasMaxLength(50);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.DiscountType)
                .HasMaxLength(20);

            builder.Property(p => p.DiscountValue)
                .HasPrecision(18, 2);

            builder.Property(p => p.MinBookingValue)
                .HasPrecision(18, 2);

            builder.Property(p => p.MaxDiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(p => p.DeletedAt == null);

            builder.HasIndex(p => p.Code)
                .IsUnique();

            // Relationships
            builder.HasOne(p => p.Field)
                .WithMany(f => f.Promotions)
                .HasForeignKey(p => p.FieldId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Bookings)
                .WithOne(b => b.Promotion)
                .HasForeignKey(b => b.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}