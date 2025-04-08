using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.RefreshTokenId);
            builder.Property(rt => rt.AccountId).IsRequired();
            builder.Property(rt => rt.Token).IsRequired().HasMaxLength(255);
            builder.Property(rt => rt.Expires).IsRequired();
            builder.Property(rt => rt.Created).IsRequired();

            builder.HasOne(rt => rt.Account)
                   .WithMany()
                   .HasForeignKey(rt => rt.AccountId);
        }
    }
}