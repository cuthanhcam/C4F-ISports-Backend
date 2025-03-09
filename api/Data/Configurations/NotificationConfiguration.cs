using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.NotificationId);
            builder.Property(n => n.UserId).IsRequired();
            builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
            builder.Property(n => n.Content).IsRequired();
            builder.Property(n => n.IsRead).IsRequired();
            builder.Property(n => n.CreatedAt).IsRequired();

            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId);
        }
    }
}