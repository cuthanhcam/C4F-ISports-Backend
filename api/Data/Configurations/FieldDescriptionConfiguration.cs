using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class FieldDescriptionConfiguration : IEntityTypeConfiguration<FieldDescription>
    {
        public void Configure(EntityTypeBuilder<FieldDescription> builder)
        {
            builder.HasKey(fd => fd.FieldDescriptionId);
            builder.HasOne(fd => fd.Field)
                   .WithMany()
                   .HasForeignKey(fd => fd.FieldId);
            builder.Property(fd => fd.Description).IsRequired();
        }
    }
}