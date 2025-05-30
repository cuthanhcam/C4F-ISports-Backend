using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
    {
        public void Configure(EntityTypeBuilder<TimeSlot> builder)
        {
            builder.HasKey(ts => ts.TimeSlotId);

            builder.Property(ts => ts.StartTime)
                .IsRequired();

            builder.Property(ts => ts.EndTime)
                .IsRequired();

            builder.Property(ts => ts.PricePerSlot)
                .IsRequired()
                .HasPrecision(18, 2);

            // Thêm relationship với PricingRule
            builder.HasOne(ts => ts.PricingRule)
                .WithMany(pr => pr.TimeSlots)
                .HasForeignKey(ts => ts.PricingRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thêm soft delete filter nếu cần
            builder.HasQueryFilter(ts => ts.DeletedAt == null);
        }
    }
}