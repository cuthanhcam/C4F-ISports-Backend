using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class UpdateTimeSlotDto
    {
        public int? TimeSlotId { get; set; } // ID của TimeSlot, dùng để xác định thực thể cần cập nhật

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mỗi khung giờ phải là số không âm.")]
        public decimal PricePerSlot { get; set; }
    }
}