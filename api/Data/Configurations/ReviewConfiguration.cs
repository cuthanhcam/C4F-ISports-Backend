using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.ReviewId);

            builder.Property(r => r.ReviewId)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.FieldId)
                .IsRequired();

            builder.Property(r => r.Rating)
                .IsRequired()
                .HasAnnotation("CheckConstraint", "Rating >= 1 AND Rating <= 5");

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(r => r.UpdatedAt)
                .IsRequired(false);

            builder.Property(r => r.OwnerReply)
                .HasMaxLength(1000);

            builder.Property(r => r.ReplyDate)
                .IsRequired(false);

            builder.Property(r => r.IsVisible)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction

            builder.HasOne(r => r.Field)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FieldId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}