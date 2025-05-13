using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
    {
        public void Configure(EntityTypeBuilder<BookingService> builder)
        {
            builder.ToTable("BookingServices");

            builder.HasKey(bs => bs.BookingServiceId);

            builder.Property(bs => bs.BookingServiceId)
                .ValueGeneratedOnAdd();

            builder.Property(bs => bs.BookingId)
                .IsRequired();

            builder.Property(bs => bs.FieldServiceId)
                .IsRequired();

            builder.Property(bs => bs.Quantity)
                .IsRequired();

            builder.Property(bs => bs.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(bs => bs.Description)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(bs => bs.FieldService)
                .WithMany(fs => fs.BookingServices)
                .HasForeignKey(bs => bs.FieldServiceId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}