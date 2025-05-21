using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldPricingDtos
{
    public class UpdateFieldPricingDto
    {
        [Required(ErrorMessage = "ID sân nhỏ là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID sân nhỏ phải là số nguyên dương.")]
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian bắt đầu phải có định dạng HH:mm.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian kết thúc phải có định dạng HH:mm.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày trong tuần là bắt buộc.")]
        [Range(0, 6, ErrorMessage = "Ngày trong tuần phải từ 0 (Chủ nhật) đến 6 (Thứ 7).")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải là số không âm.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Trạng thái giá là bắt buộc.")]
        public bool IsActive { get; set; }
    }
}