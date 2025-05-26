using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
    {
        public void Configure(EntityTypeBuilder<RefundRequest> builder)
        {
            builder.HasKey(rr => rr.RefundRequestId);

            builder.Property(rr => rr.Amount)
                .HasPrecision(18, 2);

            builder.Property(rr => rr.Reason)
                .HasMaxLength(1000);

            builder.Property(rr => rr.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(rr => rr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(rr => rr.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(rr => rr.DeletedAt == null);

            builder.HasIndex(rr => rr.PaymentId);

            // Relationships
            builder.HasOne(rr => rr.Payment)
                .WithMany(p => p.RefundRequests)
                .HasForeignKey(rr => rr.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}