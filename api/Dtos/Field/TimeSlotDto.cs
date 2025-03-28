using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class TimeSlotDto
    {
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian bắt đầu phải có định dạng hh:mm")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian kết thúc phải có định dạng hh:mm")]
        public string EndTime { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Available|Booked)$", ErrorMessage = "Trạng thái phải là 'Available' hoặc 'Booked'")]
        public string Status { get; set; }
    }
}