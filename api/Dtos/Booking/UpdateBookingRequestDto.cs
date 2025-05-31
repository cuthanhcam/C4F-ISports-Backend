using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class UpdateBookingRequestDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [RegularExpression("^(Confirmed|Pending|Cancelled)$", ErrorMessage = "Trạng thái phải là Confirmed, Pending hoặc Cancelled.")]
        public string Status { get; set; } = string.Empty;
    }
}