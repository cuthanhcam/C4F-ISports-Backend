using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Promotion
{
    public class ApplyPromotionDto
    {
        [Required(ErrorMessage = "BookingId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookingId phải là số dương.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;
    }
}