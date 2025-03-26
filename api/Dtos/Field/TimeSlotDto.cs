using System;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class TimeSlotDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 9999999.99, ErrorMessage = "Giá phải từ 0 đến 9,999,999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Trạng thái khả dụng không được để trống")]
        public bool IsAvailable { get; set; }
    }
}