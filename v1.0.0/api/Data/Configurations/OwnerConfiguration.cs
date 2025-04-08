using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
    {
        public void Configure(EntityTypeBuilder<Owner> builder)
        {
            builder.HasKey(o => o.OwnerId);
            builder.Property(o => o.AccountId).IsRequired();
            builder.Property(o => o.FullName).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Phone).HasMaxLength(20);
            builder.Property(o => o.Email).IsRequired().HasMaxLength(255);
            builder.HasIndex(o => o.Email).IsUnique();
        }
    }
}