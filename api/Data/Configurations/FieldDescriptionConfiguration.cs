using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldDescriptionConfiguration : IEntityTypeConfiguration<FieldDescription>
    {
        public void Configure(EntityTypeBuilder<FieldDescription> builder)
        {
            builder.HasKey(fd => fd.FieldDescriptionId);

            builder.Property(fd => fd.Description)
                .IsRequired()
                .HasMaxLength(2000);

            // Relationships
            builder.HasOne(fd => fd.Field)
                .WithMany(f => f.FieldDescriptions)
                .HasForeignKey(fd => fd.FieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}