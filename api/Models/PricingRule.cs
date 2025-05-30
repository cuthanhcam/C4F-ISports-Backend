using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class PricingRule
    {
        public int PricingRuleId { get; set; }

        [Required]
        public List<string> AppliesToDays { get; set; } = new List<string>(); // Monday, Tuesday, ...

        [Required]
        public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [Required]
        public int SubFieldId { get; set; }

        [ForeignKey("SubFieldId")]
        public SubField SubField { get; set; } = null!;
    }
}