using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "ID sân con là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID sân con phải là số nguyên dương.")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID người dùng phải là số nguyên dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc.")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        [MinLength(1, ErrorMessage = "Phải chỉ định ít nhất một khung giờ.")]
        public List<BookingTimeSlotResponseDto> TimeSlots { get; set; } = new();

        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc.")]
        public List<BookingServiceResponseDto> Services { get; set; } = new();

        [Required(ErrorMessage = "Tổng giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá phải là số không âm.")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Trạng thái phải là 'Pending', 'Confirmed' hoặc 'Cancelled'.")]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Trạng thái thanh toán là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Trạng thái thanh toán không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Paid|Failed)$", ErrorMessage = "Trạng thái thanh toán phải là 'Pending', 'Paid' hoặc 'Failed'.")]
        public string PaymentStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Ngày tạo là bắt buộc.")]
        public DateTime CreatedAt { get; set; }
    }
}