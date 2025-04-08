using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Booking
{
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "Danh sách sân nhỏ không được để trống")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sân nhỏ")]
        public List<SubFieldBookingDto> SubFields { get; set; }

        [Required(ErrorMessage = "Ngày đặt sân không được để trống")]
        public string BookingDate { get; set; } // YYYY-MM-DD

        public int[] ServiceIds { get; set; } // Danh sách ID dịch vụ đi kèm
    }

    public class SubFieldBookingDto
    {
        [Required(ErrorMessage = "ID sân nhỏ không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "SubFieldId phải lớn hơn 0")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Danh sách khung giờ không được để trống")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 khung giờ")]
        public List<TimeSlotCreateBookingDto> TimeSlots { get; set; }
    }

    public class TimeSlotCreateBookingDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian phải có định dạng hh:mm")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian phải có định dạng hh:mm")]
        public string EndTime { get; set; }
    }
}