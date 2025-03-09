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
            builder.Property(b => b.FieldId).IsRequired();
            builder.Property(b => b.BookingDate).IsRequired();
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.TotalPrice).HasPrecision(10, 2);
            builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
            builder.Property(b => b.PaymentStatus).IsRequired().HasMaxLength(20);
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.UpdatedAt).IsRequired();

            // Quan hệ với User và Field, sử dụng NoAction để tránh multiple cascade paths
            builder.HasOne(b => b.User)
                   .WithMany(u => u.Bookings)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.NoAction); // Thay Cascade thành NoAction

            builder.HasOne(b => b.Field)
                   .WithMany(f => f.Bookings)
                   .HasForeignKey(b => b.FieldId)
                   .OnDelete(DeleteBehavior.NoAction); // Thay Cascade thành NoAction
        }
    }
}