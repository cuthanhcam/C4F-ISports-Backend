using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin quy tắc giá.
    /// </summary>
    public class PricingRuleDto
    {
        public int PricingRuleId { get; set; }
        public List<string> AppliesToDays { get; set; } = new List<string>();
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }
}