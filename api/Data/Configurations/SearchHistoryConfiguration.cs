using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
    {
        public void Configure(EntityTypeBuilder<SearchHistory> builder)
        {
            builder.HasKey(sh => sh.SearchId);

            builder.Property(sh => sh.UserId)
                .IsRequired();

            builder.Property(sh => sh.Keyword)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(sh => sh.SearchDateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sh => sh.Latitude)
                .HasPrecision(18, 6);

            builder.Property(sh => sh.Longitude)
                .HasPrecision(18, 6);

            builder.Property(sh => sh.DeletedAt)
                .HasColumnType("datetime");

            builder.HasQueryFilter(sh => sh.DeletedAt == null);

            builder.HasIndex(sh => sh.UserId);
            builder.HasIndex(sh => sh.SearchDateTime);

            // Relationships
            builder.HasOne(sh => sh.Account)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sh => sh.Field)
                .WithMany()
                .HasForeignKey(sh => sh.FieldId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}