using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.BookingId).IsRequired();
            builder.Property(p => p.Amount).HasPrecision(10, 2);
            builder.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
            builder.Property(p => p.TransactionId).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
            builder.Property(p => p.CreatedAt).IsRequired();

            builder.HasOne(p => p.Booking)
                   .WithMany()
                   .HasForeignKey(p => p.BookingId);
        }
    }
}