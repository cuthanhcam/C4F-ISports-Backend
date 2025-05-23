using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class SubFieldConfiguration : IEntityTypeConfiguration<SubField>
    {
        public void Configure(EntityTypeBuilder<SubField> builder)
        {
            builder.ToTable("SubFields");

            builder.HasKey(sf => sf.SubFieldId);

            builder.Property(sf => sf.SubFieldId)
                .ValueGeneratedOnAdd();

            builder.Property(sf => sf.FieldId)
                .IsRequired();

            builder.Property(sf => sf.SubFieldName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sf => sf.FieldType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sf => sf.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sf => sf.Capacity)
                .IsRequired();

            builder.Property(sf => sf.Description)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(sf => sf.FieldId);

            // Relationships
            builder.HasOne(sf => sf.Field)
                .WithMany(f => f.SubFields)
                .HasForeignKey(sf => sf.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            // Sử dụng ON DELETE NO ACTION
            builder.HasMany(sf => sf.FieldPricings)
                .WithOne(fp => fp.SubField)
                .HasForeignKey(fp => fp.SubFieldId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(sf => sf.Bookings)
                .WithOne(b => b.SubField)
                .HasForeignKey(b => b.SubFieldId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}