using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FavoriteFieldConfiguration : IEntityTypeConfiguration<FavoriteField>
    {
        public void Configure(EntityTypeBuilder<FavoriteField> builder)
        {
            builder.ToTable("FavoriteFields");

            builder.HasKey(ff => ff.FavoriteId);

            builder.Property(ff => ff.FavoriteId)
                .ValueGeneratedOnAdd();

            builder.Property(ff => ff.UserId)
                .IsRequired();

            builder.Property(ff => ff.FieldId)
                .IsRequired();

            builder.Property(ff => ff.AddedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(ff => ff.User)
                .WithMany(u => u.FavoriteFields)
                .HasForeignKey(ff => ff.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(ff => ff.Field)
                .WithMany(f => f.FavoriteFields)
                .HasForeignKey(ff => ff.FieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}