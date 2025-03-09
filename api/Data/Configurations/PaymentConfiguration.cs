using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            builder.HasOne(p => p.Booking)
                   .WithMany()
                   .HasForeignKey(p => p.BookingId);
            builder.Property(p => p.Amount).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
            builder.Property(p => p.TransactionId).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Status).HasMaxLength(20).IsRequired();
            builder.Property(p => p.CreatedAt).IsRequired();
        }
    }
}