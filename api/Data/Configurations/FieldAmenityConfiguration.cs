using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldAmenityConfiguration : IEntityTypeConfiguration<FieldAmenity>
    {
        public void Configure(EntityTypeBuilder<FieldAmenity> builder)
        {
            builder.ToTable("FieldAmenities");

            builder.HasKey(fa => fa.FieldAmenityId);

            builder.Property(fa => fa.FieldAmenityId)
                .ValueGeneratedOnAdd();

            builder.Property(fa => fa.FieldId)
                .IsRequired();

            builder.Property(fa => fa.AmenityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fa => fa.Description)
                .HasMaxLength(500);

            builder.Property(fa => fa.IconUrl)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(fa => fa.Field)
                .WithMany(f => f.FieldAmenities)
                .HasForeignKey(fa => fa.FieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction để tránh lan truyền xóa
        }
    }
}