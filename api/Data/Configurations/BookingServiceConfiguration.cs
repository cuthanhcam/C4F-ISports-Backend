using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
    {
        public void Configure(EntityTypeBuilder<BookingService> builder)
        {
            builder.HasKey(bs => bs.BookingServiceId);

            builder.Property(bs => bs.Quantity)
                .IsRequired();

            builder.Property(bs => bs.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(bs => bs.Description)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bs => bs.FieldService)
                .WithMany(fs => fs.BookingServices)
                .HasForeignKey(bs => bs.FieldServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}