using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(p => p.TransactionId)
                .HasMaxLength(100);

            builder.Property(p => p.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(p => p.Currency)
                .HasMaxLength(10);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(p => p.DeletedAt == null);

            builder.HasIndex(p => p.BookingId);
            builder.HasIndex(p => p.TransactionId);

            // Relationships
            builder.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.RefundRequests)
                .WithOne(rr => rr.Payment)
                .HasForeignKey(rr => rr.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}