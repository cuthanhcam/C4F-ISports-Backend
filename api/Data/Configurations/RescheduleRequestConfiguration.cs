using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class RescheduleRequestConfiguration : IEntityTypeConfiguration<RescheduleRequest>
    {
        public void Configure(EntityTypeBuilder<RescheduleRequest> builder)
        {
            builder.HasKey(rr => rr.RescheduleRequestId);

            builder.Property(rr => rr.NewDate)
                .IsRequired();

            builder.Property(rr => rr.NewStartTime)
                .IsRequired();

            builder.Property(rr => rr.NewEndTime)
                .IsRequired();

            builder.Property(rr => rr.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(rr => rr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(rr => rr.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(rr => rr.DeletedAt == null);

            builder.HasIndex(rr => rr.BookingId);

            // Relationships
            builder.HasOne(rr => rr.Booking)
                .WithMany(b => b.RescheduleRequests)
                .HasForeignKey(rr => rr.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}