using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingId);

            builder.Property(b => b.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(b => b.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(b => b.Notes)
                .HasMaxLength(500);

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.SubFieldId);
            builder.HasIndex(b => b.PromotionId);

            // Relationships
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.SubField)
                .WithMany(sf => sf.Bookings)
                .HasForeignKey(b => b.SubFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.MainBooking)
                .WithMany(b => b.RelatedBookings)
                .HasForeignKey(b => b.MainBookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Promotion)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(b => b.BookingServices)
                .WithOne(bs => bs.Booking)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Payments)
                .WithOne(p => p.Booking)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.TimeSlots)
                .WithOne(ts => ts.Booking)
                .HasForeignKey(ts => ts.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.RescheduleRequests)
                .WithOne(rr => rr.Booking)
                .HasForeignKey(rr => rr.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Reviews)
                .WithOne(r => r.Booking)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}