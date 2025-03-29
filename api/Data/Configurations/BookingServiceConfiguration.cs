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
            builder.Property(bs => bs.FieldServiceId).IsRequired();
            builder.Property(bs => bs.Quantity).IsRequired();
            builder.Property(bs => bs.Price).HasPrecision(10, 2);

            builder.HasOne(bs => bs.Booking)
                   .WithMany(b => b.BookingServices)
                   .HasForeignKey(bs => bs.BookingId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(bs => bs.FieldService)
                   .WithMany(fs => fs.BookingServices)
                   .HasForeignKey(bs => bs.FieldServiceId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}