using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int SubFieldId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required, Range(0, 6)]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, ..., 6=Saturday

        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public SubField SubField { get; set; }
    }
}