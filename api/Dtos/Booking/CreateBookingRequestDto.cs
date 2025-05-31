using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class CreateBookingRequestDto
    {
        [Required(ErrorMessage = "Danh sách đặt sân là bắt buộc.")]
        [MaxLength(5, ErrorMessage = "Tối đa 5 đặt sân mỗi yêu cầu.")]
        public List<BookingItemRequestDto> Bookings { get; set; } = new List<BookingItemRequestDto>();

        [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
        public string? PromotionCode { get; set; }
    }
}