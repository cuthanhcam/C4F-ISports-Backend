using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
    {
        public void Configure(EntityTypeBuilder<FieldImage> builder)
        {
            builder.ToTable("FieldImages");

            builder.HasKey(fi => fi.FieldImageId);

            builder.Property(fi => fi.FieldImageId)
                .ValueGeneratedOnAdd();

            builder.Property(fi => fi.FieldId)
                .IsRequired();

            builder.Property(fi => fi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(fi => fi.Thumbnail)
                .HasMaxLength(500);

            builder.Property(fi => fi.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(fi => fi.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(fi => fi.Field)
                .WithMany(f => f.FieldImages)
                .HasForeignKey(fi => fi.FieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}