using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
    {
        public void Configure(EntityTypeBuilder<FieldImage> builder)
        {
            builder.HasKey(fi => fi.FieldImageId);

            builder.Property(fi => fi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(fi => fi.PublicId)
                .HasMaxLength(500);

            builder.Property(fi => fi.Thumbnail)
                .HasMaxLength(500);

            builder.Property(fi => fi.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fi => fi.Field)
                .WithMany(f => f.FieldImages)
                .HasForeignKey(fi => fi.FieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}