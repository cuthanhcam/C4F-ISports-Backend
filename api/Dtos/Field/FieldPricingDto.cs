using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldPricingDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Ngày trong tuần không được để trống")]
        [Range(0, 6, ErrorMessage = "Ngày trong tuần không hợp lệ")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0, 9999999.99, ErrorMessage = "Giá phải từ 0 đến 9,999,999.99")]
        public decimal Price { get; set; }
    }
}