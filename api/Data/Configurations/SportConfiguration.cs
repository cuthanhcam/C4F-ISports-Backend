using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class SportConfiguration : IEntityTypeConfiguration<Sport>
    {
        public void Configure(EntityTypeBuilder<Sport> builder)
        {
            builder.ToTable("Sports");

            builder.HasKey(s => s.SportId);

            builder.Property(s => s.SportId)
                .ValueGeneratedOnAdd();

            builder.Property(s => s.SportName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            builder.Property(s => s.IconUrl)
                .HasMaxLength(500);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            // Sử dụng ON DELETE NO ACTION cho Fields
            builder.HasMany(s => s.Fields)
                .WithOne(f => f.Sport)
                .HasForeignKey(f => f.SportId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}