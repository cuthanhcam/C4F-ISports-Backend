using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Booking
{
    public class UpdateBookingDto
    {
        [Required(ErrorMessage = "Ngày đặt sân không được để trống")]
        public string BookingDate { get; set; } // YYYY-MM-DD

        [Required(ErrorMessage = "Danh sách sân nhỏ không được để trống")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sân nhỏ")]
        public List<SubFieldBookingUpdateDto> SubFields { get; set; }

        public int[] ServiceIds { get; set; } // Danh sách ID dịch vụ đi kèm
    }

    public class SubFieldBookingUpdateDto
    {
        [Required(ErrorMessage = "ID sân nhỏ không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "SubFieldId phải lớn hơn 0")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Danh sách khung giờ không được để trống")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 khung giờ")]
        public List<TimeSlotCreateBookingDto> TimeSlots { get; set; }
    }
}