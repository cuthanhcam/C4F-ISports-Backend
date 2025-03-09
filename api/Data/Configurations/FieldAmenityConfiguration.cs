using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldAmenityConfiguration : IEntityTypeConfiguration<FieldAmenity>
    {
        public void Configure(EntityTypeBuilder<FieldAmenity> builder)
        {
            builder.HasKey(fa => fa.FieldAmenityId);
            builder.Property(fa => fa.FieldId).IsRequired();
            builder.Property(fa => fa.AmenityName).IsRequired().HasMaxLength(100);

            builder.HasOne(fa => fa.Field)
                   .WithMany()
                   .HasForeignKey(fa => fa.FieldId);
        }
    }
}