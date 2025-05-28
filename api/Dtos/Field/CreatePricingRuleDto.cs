using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để tạo quy tắc giá.
    /// </summary>
    public class CreatePricingRuleDto
    {
        [Required(ErrorMessage = "Ngày áp dụng là bắt buộc.")]
        public List<string> AppliesToDays { get; set; } = new List<string>();

        [Required(ErrorMessage = "Khung giờ là bắt buộc.")]
        public List<CreateTimeSlotDto> TimeSlots { get; set; } = new List<CreateTimeSlotDto>();
    }
}