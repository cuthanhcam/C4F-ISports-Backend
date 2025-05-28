using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin sân con.
    /// </summary>
    public class SubFieldDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public int Capacity { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public decimal DefaultPricePerSlot { get; set; }
        public int? Parent7aSideId { get; set; }
        public List<int> Child5aSideIds { get; set; } = new List<int>();
        public List<PricingRuleDto> PricingRules { get; set; } = new List<PricingRuleDto>();
    }
}