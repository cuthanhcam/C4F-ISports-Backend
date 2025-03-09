using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldPricingConfiguration : IEntityTypeConfiguration<FieldPricing>
    {
        public void Configure(EntityTypeBuilder<FieldPricing> builder)
        {
            builder.HasKey(fp => fp.FieldPricingId);
            builder.Property(fp => fp.FieldId).IsRequired();
            builder.Property(fp => fp.StartTime).IsRequired();
            builder.Property(fp => fp.EndTime).IsRequired();
            builder.Property(fp => fp.DayOfWeek).IsRequired();
            builder.Property(fp => fp.Price).HasPrecision(10, 2);

            builder.HasOne(fp => fp.Field)
                   .WithMany()
                   .HasForeignKey(fp => fp.FieldId);
        }
    }
}