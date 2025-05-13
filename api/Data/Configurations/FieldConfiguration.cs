using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> builder)
        {
            builder.ToTable("Fields");

            builder.HasKey(f => f.FieldId);

            builder.Property(f => f.FieldId)
                .ValueGeneratedOnAdd();

            builder.Property(f => f.SportId)
                .IsRequired();

            builder.Property(f => f.FieldName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.OpenHours)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.OpenTime)
                .IsRequired(false);

            builder.Property(f => f.CloseTime)
                .IsRequired(false);

            builder.Property(f => f.OwnerId)
                .IsRequired();

            builder.Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.Latitude)
                .IsRequired()
                .HasColumnType("decimal(9,6)");

            builder.Property(f => f.Longitude)
                .IsRequired()
                .HasColumnType("decimal(9,6)");

            builder.Property(f => f.City)
                .HasMaxLength(100);

            builder.Property(f => f.District)
                .HasMaxLength(100);

            builder.Property(f => f.AverageRating)
                .HasColumnType("decimal(3,1)");

            builder.Property(f => f.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(f => f.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(f => new { f.Latitude, f.Longitude });

            // Relationships
            builder.HasOne(f => f.Sport)
                .WithMany(s => s.Fields)
                .HasForeignKey(f => f.SportId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.Owner)
                .WithMany(o => o.Fields)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Sử dụng ON DELETE NO ACTION cho các mối quan hệ nhiều-nhiều
            builder.HasMany(f => f.SubFields)
                .WithOne(sf => sf.Field)
                .HasForeignKey(sf => sf.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.Reviews)
                .WithOne(r => r.Field)
                .HasForeignKey(r => r.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.FieldImages)
                .WithOne(fi => fi.Field)
                .HasForeignKey(fi => fi.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.FieldAmenities)
                .WithOne(fa => fa.Field)
                .HasForeignKey(fa => fa.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.FieldDescriptions)
                .WithOne(fd => fd.Field)
                .HasForeignKey(fd => fd.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.FieldServices)
                .WithOne(fs => fs.Field)
                .HasForeignKey(fs => fs.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(f => f.FavoriteFields)
                .WithOne(ff => ff.Field)
                .HasForeignKey(ff => ff.FieldId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}