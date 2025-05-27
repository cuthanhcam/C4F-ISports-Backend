using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingTimeSlotConfiguration : IEntityTypeConfiguration<BookingTimeSlot>
    {
        public void Configure(EntityTypeBuilder<BookingTimeSlot> builder)
        {
            builder.HasKey(bts => bts.BookingTimeSlotId);

            builder.Property(bts => bts.StartTime)
                .IsRequired();

            builder.Property(bts => bts.EndTime)
                .IsRequired();

            builder.Property(bts => bts.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(bts => bts.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(bts => bts.DeletedAt == null);

            // Relationships
            builder.HasOne(bts => bts.Booking)
                .WithMany(b => b.TimeSlots)
                .HasForeignKey(bts => bts.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}