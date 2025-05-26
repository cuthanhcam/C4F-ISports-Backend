using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FavoriteFieldConfiguration : IEntityTypeConfiguration<FavoriteField>
    {
        public void Configure(EntityTypeBuilder<FavoriteField> builder)
        {
            builder.HasKey(ff => ff.FavoriteId);

            builder.Property(ff => ff.AddedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(ff => ff.User)
                .WithMany(u => u.FavoriteFields)
                .HasForeignKey(ff => ff.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ff => ff.Field)
                .WithMany(f => f.FavoriteFields)
                .HasForeignKey(ff => ff.FieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}