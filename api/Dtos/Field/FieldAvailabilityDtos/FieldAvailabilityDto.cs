using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldAvailabilityDtos
{
    public class FieldAvailabilityDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public PromotionDto? Promotion { get; set; }
    }
}