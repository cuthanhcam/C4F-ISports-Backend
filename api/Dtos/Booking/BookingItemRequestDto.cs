using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class BookingItemRequestDto
    {
        [Required(ErrorMessage = "SubFieldId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "SubFieldId phải là số dương.")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Danh sách time slot là bắt buộc.")]
        [MaxLength(10, ErrorMessage = "Tối đa 10 time slot mỗi đặt sân.")]
        public List<TimeSlotRequestDto> TimeSlots { get; set; } = new List<TimeSlotRequestDto>();

        [MaxLength(20, ErrorMessage = "Tối đa 20 dịch vụ mỗi đặt sân.")]
        public List<BookingServiceRequestDto>? Services { get; set; }
    }
}