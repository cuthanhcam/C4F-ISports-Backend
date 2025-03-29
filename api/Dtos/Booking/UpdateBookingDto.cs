using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Booking
{
    public class UpdateBookingDto
    {
        [Required(ErrorMessage = "Ngày đặt sân không được để trống")]
        public string BookingDate { get; set; } // YYYY-MM-DD

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian bắt đầu phải có định dạng hh:mm")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian kết thúc phải có định dạng hh:mm")]
        public string EndTime { get; set; }

        public int[] ServiceIds { get; set; } // Danh sách ID dịch vụ đi kèm, có thể để trống
    }
}