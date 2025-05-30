using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Promotion
{
    public class UpdatePromotionDto
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại giảm giá là bắt buộc.")]
        [RegularExpression("^(Percentage|Fixed)$", ErrorMessage = "Loại giảm giá phải là Percentage hoặc Fixed.")]
        public string DiscountType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải là số không âm.")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải là số không âm.")]
        public int? UsageLimit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải là số không âm.")]
        public decimal? MinBookingValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải là số không âm.")]
        public decimal? MaxDiscountAmount { get; set; }
    }
}