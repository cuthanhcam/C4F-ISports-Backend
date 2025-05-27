using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.RefreshTokenId);

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(rt => rt.Expires)
                .IsRequired();

            builder.Property(rt => rt.Created)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(rt => rt.ReplacedByToken)
                .HasMaxLength(256);

            // Relationships
            builder.HasOne(rt => rt.Account)
                .WithMany(a => a.RefreshTokens)
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}