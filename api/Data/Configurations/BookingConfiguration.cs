using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingId);

            builder.HasOne(b => b.User)
               .WithMany()
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.NoAction); // Ngăn chặn lỗi nhiều Cascade

            builder.HasOne(b => b.Field)
                   .WithMany()
                   .HasForeignKey(b => b.FieldId)
                   .OnDelete(DeleteBehavior.NoAction); // Ngăn chặn lỗi nhiều Cascade
                   
            builder.Property(b => b.BookingDate).IsRequired();
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.TotalPrice).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(b => b.Status).HasMaxLength(20).IsRequired();
            builder.Property(b => b.PaymentStatus).HasMaxLength(20).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.UpdatedAt).IsRequired();
        }
    }
}