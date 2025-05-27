using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
    {
        public void Configure(EntityTypeBuilder<Owner> builder)
        {
            builder.HasKey(o => o.OwnerId);

            builder.Property(o => o.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.Description)
                .HasMaxLength(1000);

            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(o => o.Account)
                .WithOne(a => a.Owner)
                .HasForeignKey<Owner>(o => o.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Fields)
                .WithOne(f => f.Owner)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}