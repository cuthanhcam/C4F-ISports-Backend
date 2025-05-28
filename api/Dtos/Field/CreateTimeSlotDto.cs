using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để tạo khung giờ giá.
    /// </summary>
    public class CreateTimeSlotDto
    {
        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số dương.")]
        public decimal PricePerSlot { get; set; }
    }
}