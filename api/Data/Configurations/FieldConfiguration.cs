using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.HasKey(f => f.FieldId);

            builder.Property(f => f.FieldName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Description)
                .HasMaxLength(500);

            builder.Property(f => f.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.District)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(f => f.AverageRating)
                .HasPrecision(3, 2);

            builder.Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(f => f.SportId);
            builder.HasIndex(f => f.OwnerId);
            builder.HasIndex(f => f.City);
            builder.HasIndex(f => f.District);

            // Relationships
            builder.HasOne(f => f.Sport)
                .WithMany()
                .HasForeignKey(f => f.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add soft delete filter
            builder.HasQueryFilter(f => f.DeletedAt == null);
        }
    }
}