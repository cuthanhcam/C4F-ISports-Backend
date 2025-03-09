using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldDescriptionConfiguration : IEntityTypeConfiguration<FieldDescription>
    {
        public void Configure(EntityTypeBuilder<FieldDescription> builder)
        {
            builder.HasKey(fd => fd.FieldDescriptionId);
            builder.Property(fd => fd.FieldId).IsRequired();
            builder.Property(fd => fd.Description).IsRequired();

            builder.HasOne(fd => fd.Field)
                   .WithMany()
                   .HasForeignKey(fd => fd.FieldId);
        }
    }
}