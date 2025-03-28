using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using api.Models;

namespace api.Data.Configurations
{
    public class SubFieldConfiguration : IEntityTypeConfiguration<SubField>
    {
        public void Configure(EntityTypeBuilder<SubField> builder)
        {
            builder.Property(sf => sf.SubFieldName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(sf => sf.Size)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(sf => sf.PricePerHour)
                   .IsRequired();

            builder.Property(sf => sf.Status)
                   .IsRequired()
                   .HasMaxLength(20);
        }
    }
}