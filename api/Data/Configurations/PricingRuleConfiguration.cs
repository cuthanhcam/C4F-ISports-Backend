using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace api.Data.Configurations
{
    public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
    {
        public void Configure(EntityTypeBuilder<PricingRule> builder)
        {
            builder.HasKey(pr => pr.PricingRuleId);

            // Configure AppliesToDays as a JSON string
            builder.Property(pr => pr.AppliesToDays)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

            // Relationship with SubField
            builder.HasOne(pr => pr.SubField)
                .WithMany(sf => sf.PricingRules)
                .HasForeignKey(pr => pr.SubFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with TimeSlots - một PricingRule có nhiều TimeSlot
            builder.HasMany(pr => pr.TimeSlots)
                .WithOne(ts => ts.PricingRule)
                .HasForeignKey(ts => ts.PricingRuleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Defining the many-to-many relationship between PricingRule and TimeSlot
            // You'll need a join entity to represent this relationship in the database
            // builder.HasMany(pr => pr.TimeSlots)
            //     .WithMany()
            //     .UsingEntity<Dictionary<string, object>>(
            //         "PricingRuleTimeSlot",
            //         j => j.HasOne<TimeSlot>().WithMany().OnDelete(DeleteBehavior.Cascade),
            //         j => j.HasOne<PricingRule>().WithMany().OnDelete(DeleteBehavior.Cascade)
            //     );
        }
    }
}