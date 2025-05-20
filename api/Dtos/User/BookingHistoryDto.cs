using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class BookingHistoryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "BookingId phải là số dương.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sân phụ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân phụ không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc.")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        public TimeSpan EndTime { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá phải là số không âm.")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [RegularExpression("^(Confirmed|Pending|Cancelled)$", ErrorMessage = "Trạng thái không hợp lệ. Chỉ chấp nhận 'Confirmed', 'Pending' hoặc 'Cancelled'.")]
        public string Status { get; set; } = string.Empty;
    }
}