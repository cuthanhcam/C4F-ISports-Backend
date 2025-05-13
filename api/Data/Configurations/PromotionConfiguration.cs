using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");

            builder.HasKey(p => p.PromotionId);

            builder.Property(p => p.PromotionId)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.DiscountType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.DiscountValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.StartDate)
                .IsRequired();

            builder.Property(p => p.EndDate)
                .IsRequired();

            builder.Property(p => p.MinBookingValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.MaxDiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.UsageLimit)
                .IsRequired(false);

            builder.Property(p => p.UsageCount)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(p => p.Code)
                .IsUnique();

            // Relationships
            builder.HasMany(p => p.Bookings)
                .WithOne(b => b.Promotion)
                .HasForeignKey(b => b.PromotionId)
                .OnDelete(DeleteBehavior.SetNull); // Giữ SetNull
        }
    }
}