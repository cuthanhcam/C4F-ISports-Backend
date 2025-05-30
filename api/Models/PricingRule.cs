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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [Required]
        public int SubFieldId { get; set; }

        [ForeignKey("SubFieldId")]
        public SubField SubField { get; set; } = null!;

        [Required]
        public List<string> AppliesToDays { get; set; } = new List<string>(); // Monday, Tuesday, ...
        
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}