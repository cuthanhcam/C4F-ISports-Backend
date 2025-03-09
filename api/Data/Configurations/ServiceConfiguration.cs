using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.ServiceId);
            builder.HasOne(s => s.Field)
                   .WithMany()
                   .HasForeignKey(s => s.FieldId);
            builder.Property(s => s.ServiceName).HasMaxLength(100).IsRequired();
            builder.Property(s => s.Price).HasColumnType("decimal(10,2)").IsRequired();
        }
    }
}