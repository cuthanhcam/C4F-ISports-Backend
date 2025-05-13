using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.BookingId);

            builder.Property(b => b.BookingId)
                .ValueGeneratedOnAdd();

            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.SubFieldId)
                .IsRequired();

            builder.Property(b => b.MainBookingId)
                .IsRequired(false);

            builder.Property(b => b.BookingDate)
                .IsRequired();

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasAnnotation("CheckConstraint", "Status IN ('Confirmed', 'Pending', 'Cancelled')");

            builder.Property(b => b.PaymentStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasAnnotation("CheckConstraint", "PaymentStatus IN ('Paid', 'Pending', 'Failed')");

            builder.Property(b => b.Notes)
                .HasMaxLength(1000);

            builder.Property(b => b.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(b => b.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(b => b.IsReminderSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(b => b.PromotionId)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(b => new { b.SubFieldId, b.BookingDate, b.StartTime, b.EndTime });

            // Relationships
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(b => b.SubField)
                .WithMany(sf => sf.Bookings)
                .HasForeignKey(b => b.SubFieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(b => b.MainBooking)
                .WithMany(b => b.RelatedBookings)
                .HasForeignKey(b => b.MainBookingId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(b => b.Promotion)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PromotionId)
                .OnDelete(DeleteBehavior.SetNull); // Giữ SetNull

            builder.HasMany(b => b.BookingServices)
                .WithOne(bs => bs.Booking)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasMany(b => b.Payments)
                .WithOne(p => p.Booking)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasMany(b => b.TimeSlots)
                .WithOne(bt => bt.Booking)
                .HasForeignKey(bt => bt.BookingId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}