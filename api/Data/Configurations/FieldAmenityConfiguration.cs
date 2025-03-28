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
            builder.Property(fa => fa.Description).HasMaxLength(200);

            builder.HasOne(fa => fa.Field)
                   .WithMany(f => f.FieldAmenities)
                   .HasForeignKey(fa => fa.FieldId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}