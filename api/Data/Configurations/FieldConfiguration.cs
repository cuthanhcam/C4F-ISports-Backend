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
                .HasMaxLength(100);

            builder.Property(f => f.Phone)
                .HasMaxLength(20);

            builder.Property(f => f.Address)
                .HasMaxLength(500);

            builder.Property(f => f.OpenHours)
                .HasMaxLength(50);

            builder.Property(f => f.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(f => f.City)
                .HasMaxLength(100);

            builder.Property(f => f.District)
                .HasMaxLength(100);

            builder.Property(f => f.AverageRating)
                .HasPrecision(18, 2);

            builder.Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(f => f.SportId);
            builder.HasIndex(f => f.OwnerId);
            builder.HasIndex(f => f.City);
            builder.HasIndex(f => f.District);

            // Relationships
            builder.HasOne(f => f.Sport)
                .WithMany(s => s.Fields)
                .HasForeignKey(f => f.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Owner)
                .WithMany(o => o.Fields)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(f => f.SubFields)
                .WithOne(sf => sf.Field)
                .HasForeignKey(sf => sf.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.Reviews)
                .WithOne(r => r.Field)
                .HasForeignKey(r => r.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.FieldImages)
                .WithOne(fi => fi.Field)
                .HasForeignKey(fi => fi.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.FieldAmenities)
                .WithOne(fa => fa.Field)
                .HasForeignKey(fa => fa.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.FieldDescriptions)
                .WithOne(fd => fd.Field)
                .HasForeignKey(fd => fd.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.FieldServices)
                .WithOne(fs => fs.Field)
                .HasForeignKey(fs => fs.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.FavoriteFields)
                .WithOne(ff => ff.Field)
                .HasForeignKey(ff => ff.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.Promotions)
                .WithOne(p => p.Field)
                .HasForeignKey(p => p.FieldId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}