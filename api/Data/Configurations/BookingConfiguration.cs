using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingId);
            builder.Property(b => b.UserId).IsRequired();
            builder.Property(b => b.SubFieldId).IsRequired();
            builder.Property(b => b.BookingDate).IsRequired();
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.TotalPrice).HasPrecision(10, 2);
            builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
            builder.Property(b => b.PaymentStatus).IsRequired().HasMaxLength(20);
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.UpdatedAt).IsRequired();

            // Quan hệ với User
            builder.HasOne(b => b.User)
                   .WithMany(u => u.Bookings)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Quan hệ với SubField
            builder.HasOne(b => b.SubField)
                   .WithMany(sf => sf.Bookings)
                   .HasForeignKey(b => b.SubFieldId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Quan hệ với BookingServices
            builder.HasMany(b => b.BookingServices)
                   .WithOne(bs => bs.Booking)
                   .HasForeignKey(bs => bs.BookingId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Quan hệ tự tham chiếu (booking chính - booking phụ)
            builder.HasMany(b => b.RelatedBookings)
                   .WithOne(b => b.MainBooking)
                   .HasForeignKey(b => b.MainBookingId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}