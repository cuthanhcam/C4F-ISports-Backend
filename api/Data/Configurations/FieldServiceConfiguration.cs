using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldServiceConfiguration : IEntityTypeConfiguration<FieldService>
    {
        public void Configure(EntityTypeBuilder<FieldService> builder)
        {
            builder.ToTable("FieldServices");

            builder.HasKey(fs => fs.FieldServiceId);

            builder.Property(fs => fs.FieldServiceId)
                .ValueGeneratedOnAdd();

            builder.Property(fs => fs.FieldId)
                .IsRequired();

            builder.Property(fs => fs.ServiceName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fs => fs.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(fs => fs.Description)
                .HasMaxLength(500);

            builder.Property(fs => fs.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(fs => fs.Field)
                .WithMany(f => f.FieldServices)
                .HasForeignKey(fs => fs.FieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasMany(fs => fs.BookingServices)
                .WithOne(bs => bs.FieldService)
                .HasForeignKey(bs => bs.FieldServiceId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}