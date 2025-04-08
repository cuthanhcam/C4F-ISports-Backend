using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class BookingTimeSlotConfiguration : IEntityTypeConfiguration<BookingTimeSlot>
    {
        public void Configure(EntityTypeBuilder<BookingTimeSlot> builder)
        {
            builder.HasKey(b => b.BookingTimeSlotId);
            
            builder.Property(b => b.BookingId).IsRequired();
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.Price).HasPrecision(10, 2).IsRequired();

            builder.HasOne(b => b.Booking)
                .WithMany(b => b.TimeSlots)
                .HasForeignKey(b => b.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}