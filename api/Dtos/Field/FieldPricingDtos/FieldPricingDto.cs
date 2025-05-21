using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldPricingDtos
{
    public class FieldPricingDto
    {
        public int FieldPricingId { get; set; }
        public int SubFieldId { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}