using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.ServiceId);
            builder.Property(s => s.FieldId).IsRequired();
            builder.Property(s => s.ServiceName).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Price).HasPrecision(10, 2);

            builder.HasOne(s => s.Field)
                   .WithMany()
                   .HasForeignKey(s => s.FieldId);
        }
    }
}