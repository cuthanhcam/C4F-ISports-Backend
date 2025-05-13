using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
    {
        public void Configure(EntityTypeBuilder<SearchHistory> builder)
        {
            builder.ToTable("SearchHistories");

            builder.HasKey(sh => sh.SearchHistoryId);

            builder.Property(sh => sh.SearchHistoryId)
                .ValueGeneratedOnAdd();

            builder.Property(sh => sh.UserId)
                .IsRequired();

            builder.Property(sh => sh.SearchQuery)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(sh => sh.SearchDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(sh => sh.FieldId)
                .IsRequired(false);

            builder.Property(sh => sh.Latitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(sh => sh.Longitude)
                .HasColumnType("decimal(9,6)");

            // Relationships
            builder.HasOne(sh => sh.User)
                .WithMany(u => u.SearchHistories)
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(sh => sh.Field)
                .WithMany()
                .HasForeignKey(sh => sh.FieldId)
                .OnDelete(DeleteBehavior.SetNull); // Giữ SetNull
        }
    }
}