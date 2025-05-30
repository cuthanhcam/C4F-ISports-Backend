using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreatePricingRuleDto
    {
        [Required(ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một ngày.")]
        public List<string> AppliesToDays { get; set; } = new();
        
        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 khung giờ được phép.")]
        public List<CreateTimeSlotDto> TimeSlots { get; set; } = new();
    }
}