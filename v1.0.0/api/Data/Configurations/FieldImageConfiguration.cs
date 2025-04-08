using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
    {
        public void Configure(EntityTypeBuilder<FieldImage> builder)
        {
            builder.HasKey(fi => fi.FieldImageId);
            builder.Property(fi => fi.FieldId).IsRequired();
            builder.Property(fi => fi.Thumbnail).HasMaxLength(255);
            builder.Property(fi => fi.ImageUrl).IsRequired();

            builder.HasOne(fi => fi.Field)
                   .WithMany()
                   .HasForeignKey(fi => fi.FieldId);
        }
    }
}