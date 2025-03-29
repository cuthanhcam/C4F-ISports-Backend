using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Booking
{
    public class UpdateBookingStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Pending|Confirmed|Canceled|Completed)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }
    }
}