using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldServiceConfiguration : IEntityTypeConfiguration<FieldService>
    {
        public void Configure(EntityTypeBuilder<FieldService> builder)
        {
            builder.HasKey(fs => fs.FieldServiceId);
            builder.Property(fs => fs.FieldId).IsRequired();
            builder.Property(fs => fs.ServiceName).IsRequired().HasMaxLength(100);
            builder.Property(fs => fs.Price).HasPrecision(10, 2);
            builder.Property(fs => fs.Description).HasMaxLength(200);

            builder.HasOne(fs => fs.Field)
                   .WithMany(f => f.FieldServices)
                   .HasForeignKey(fs => fs.FieldId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}