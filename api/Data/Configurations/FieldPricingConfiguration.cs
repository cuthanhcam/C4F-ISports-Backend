using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldPricingConfiguration : IEntityTypeConfiguration<FieldPricing>
    {
        public void Configure(EntityTypeBuilder<FieldPricing> builder)
        {
            builder.HasKey(fp => fp.FieldPricingId);

            builder.Property(fp => fp.StartTime)
                .IsRequired();

            builder.Property(fp => fp.EndTime)
                .IsRequired();

            builder.Property(fp => fp.DayOfWeek)
                .IsRequired();

            builder.Property(fp => fp.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(fp => fp.IsActive)
                .HasDefaultValue(true);

            // Relationship
            builder.HasOne(fp => fp.SubField)
                .WithMany()
                .HasForeignKey(fp => fp.SubFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            builder.HasQueryFilter(fp => fp.DeletedAt == null);
        }
    }
}