using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.RefreshTokenId);

            builder.Property(rt => rt.RefreshTokenId)
                .ValueGeneratedOnAdd();

            builder.Property(rt => rt.AccountId)
                .IsRequired();

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(rt => rt.Expires)
                .IsRequired();

            builder.Property(rt => rt.Created)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(rt => rt.Revoked)
                .IsRequired(false);

            builder.Property(rt => rt.ReplacedByToken)
                .HasMaxLength(256);

            // Indexes
            builder.HasIndex(rt => rt.Token)
                .IsUnique();

            // Relationships
            builder.HasOne(rt => rt.Account)
                .WithMany(a => a.RefreshTokens)
                .HasForeignKey(rt => rt.AccountId)
                .OnDelete(DeleteBehavior.NoAction); // Sử dụng NoAction
        }
    }
}