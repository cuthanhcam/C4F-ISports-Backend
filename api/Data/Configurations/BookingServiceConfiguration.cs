using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            builder.HasOne(bs => bs.Booking)
                   .WithMany()
                   .HasForeignKey(bs => bs.BookingId);
            builder.HasOne(bs => bs.Service)
                   .WithMany()
                   .HasForeignKey(bs => bs.ServiceId);
            builder.Property(bs => bs.Quantity).IsRequired();
            builder.Property(bs => bs.Price).HasColumnType("decimal(10,2)").IsRequired();
        }
    }
}