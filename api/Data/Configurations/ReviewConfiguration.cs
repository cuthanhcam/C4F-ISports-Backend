using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);

            builder.HasOne(r => r.User)
              .WithMany()
              .HasForeignKey(r => r.UserId)
              .OnDelete(DeleteBehavior.NoAction); // Ngăn chặn lỗi nhiều Cascade

            builder.HasOne(r => r.Field)
                   .WithMany()
                   .HasForeignKey(r => r.FieldId)
                   .OnDelete(DeleteBehavior.NoAction); // Ngăn chặn lỗi nhiều Cascade

            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();
        }
    }
}