using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
    {
        public void Configure(EntityTypeBuilder<BookingService> builder)
        {
            builder.HasKey(bs => bs.BookingServiceId);
            builder.Property(bs => bs.BookingId).IsRequired();
            builder.Property(bs => bs.ServiceId).IsRequired();
            builder.Property(bs => bs.Quantity).IsRequired();
            builder.Property(bs => bs.Price).HasPrecision(10, 2);

            builder.HasOne(bs => bs.Booking)
                   .WithMany()
                   .HasForeignKey(bs => bs.BookingId)
                   .OnDelete(DeleteBehavior.NoAction); // Thêm NoAction để tránh cascade

            builder.HasOne(bs => bs.Service)
                   .WithMany(s => s.BookingServices)
                   .HasForeignKey(bs => bs.ServiceId)
                   .OnDelete(DeleteBehavior.NoAction); // Thêm NoAction để tránh cascade
        }
    }
}