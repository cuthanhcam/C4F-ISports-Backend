using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace api.Data.Configurations
{
    public class SubFieldConfiguration : IEntityTypeConfiguration<SubField>
    {
        public void Configure(EntityTypeBuilder<SubField> builder)
        {
            builder.HasKey(sf => sf.SubFieldId);

            builder.Property(sf => sf.SubFieldName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sf => sf.FieldType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sf => sf.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(sf => sf.Description)
                .HasMaxLength(500);

            builder.Property(sf => sf.DefaultPricePerSlot)
                .HasPrecision(18, 2);

            // Store Child5aSideIds as JSON array
            builder.Property(sf => sf.Child5aSideIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null) ?? new List<int>());

            // Relationships
            builder.HasOne(sf => sf.Field)
                .WithMany()
                .HasForeignKey(sf => sf.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sf => sf.Parent7aSide)
                .WithMany()
                .HasForeignKey(sf => sf.Parent7aSideId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}