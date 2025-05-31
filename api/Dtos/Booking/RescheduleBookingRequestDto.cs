using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class RescheduleBookingRequestDto
    {
        [Required(ErrorMessage = "Ngày mới là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime NewDate { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu mới là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội của 30 phút.")]
        public string NewStartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc mới là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội của 30 phút.")]
        public string NewEndTime { get; set; } = string.Empty;
    }
}