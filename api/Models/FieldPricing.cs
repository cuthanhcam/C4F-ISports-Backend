using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int FieldId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DayOfWeek { get; set; }
        public decimal Price { get; set; }

        public Field Field { get; set; }
    }
}