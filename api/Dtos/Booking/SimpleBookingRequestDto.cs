using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class SimpleBookingRequestDto
    {
        [Required(ErrorMessage = "SubFieldId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "SubFieldId phải là số dương.")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội của 30 phút.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội của 30 phút.")]
        public string EndTime { get; set; } = string.Empty;
    }
}