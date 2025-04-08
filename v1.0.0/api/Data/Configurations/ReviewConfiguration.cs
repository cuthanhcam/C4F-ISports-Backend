using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);
            builder.Property(r => r.UserId).IsRequired();
            builder.Property(r => r.FieldId).IsRequired();
            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();

            // Quan hệ với User và Field, sử dụng NoAction để tránh multiple cascade paths
            builder.HasOne(r => r.User)
                   .WithMany(u => u.Reviews)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.NoAction); // Thay Cascade thành NoAction

            builder.HasOne(r => r.Field)
                   .WithMany(f => f.Reviews)
                   .HasForeignKey(r => r.FieldId)
                   .OnDelete(DeleteBehavior.NoAction); // Thay Cascade thành NoAction
        }
    }
}