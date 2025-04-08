using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.HasKey(f => f.FieldId);
            builder.Property(f => f.SportId).IsRequired();
            builder.Property(f => f.FieldName).IsRequired().HasMaxLength(100);
            builder.Property(f => f.Phone).HasMaxLength(20);
            builder.Property(f => f.Address).HasMaxLength(255);
            builder.Property(f => f.OpenHours).HasMaxLength(100);
            builder.Property(f => f.OwnerId).IsRequired();
            builder.Property(f => f.Status).IsRequired().HasMaxLength(20);
            builder.Property(f => f.Latitude).HasPrecision(9, 6);
            builder.Property(f => f.Longitude).HasPrecision(9, 6);
            builder.Property(f => f.CreatedAt).IsRequired();
            builder.Property(f => f.UpdatedAt).IsRequired();

            builder.HasIndex(f => f.FieldId);
            builder.HasOne(f => f.Sport)
                .WithMany(s => s.Fields)
                .HasForeignKey(f => f.SportId);
            builder.HasOne(f => f.Owner)
                .WithMany(o => o.Fields)
                .HasForeignKey(f => f.OwnerId);
            builder.HasMany(f => f.SubFields)
                .WithOne(sf => sf.Field)
                .HasForeignKey(sf => sf.FieldId);
        }
    }
}