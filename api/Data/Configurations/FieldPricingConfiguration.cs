using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldPricingConfiguration : IEntityTypeConfiguration<FieldPricing>
    {
        public void Configure(EntityTypeBuilder<FieldPricing> builder)
        {
            builder.ToTable("FieldPricings");

            builder.HasKey(fp => fp.FieldPricingId);

            builder.Property(fp => fp.FieldPricingId)
                .ValueGeneratedOnAdd();

            builder.Property(fp => fp.SubFieldId)
                .IsRequired();

            builder.Property(fp => fp.StartTime)
                .IsRequired();

            builder.Property(fp => fp.EndTime)
                .IsRequired();

            builder.Property(fp => fp.DayOfWeek)
                .IsRequired()
                .HasConversion<int>()
                .HasAnnotation("CheckConstraint", "DayOfWeek >= 0 AND DayOfWeek <= 6");

            builder.Property(fp => fp.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(fp => fp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(fp => new { fp.SubFieldId, fp.StartTime, fp.EndTime });

            // Relationships
            builder.HasOne(fp => fp.SubField)
                .WithMany(sf => sf.FieldPricings)
                .HasForeignKey(fp => fp.SubFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}