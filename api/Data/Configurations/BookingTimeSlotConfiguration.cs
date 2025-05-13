using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingTimeSlotConfiguration : IEntityTypeConfiguration<BookingTimeSlot>
    {
        public void Configure(EntityTypeBuilder<BookingTimeSlot> builder)
        {
            builder.ToTable("BookingTimeSlots");

            builder.HasKey(bt => bt.BookingTimeSlotId);

            builder.Property(bt => bt.BookingTimeSlotId)
                .ValueGeneratedOnAdd();

            builder.Property(bt => bt.BookingId)
                .IsRequired();

            builder.Property(bt => bt.StartTime)
                .IsRequired();

            builder.Property(bt => bt.EndTime)
                .IsRequired();

            builder.Property(bt => bt.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(bt => bt.Booking)
                .WithMany(b => b.TimeSlots)
                .HasForeignKey(bt => bt.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}