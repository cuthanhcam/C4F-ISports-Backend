using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để tạo sân con.
    /// </summary>
    public class CreateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân con là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân con không được vượt quá 50 ký tự.")]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Sức chứa phải từ 1 đến 100 người.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Giờ mở cửa là bắt buộc.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giờ đóng cửa là bắt buộc.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mặc định là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số dương.")]
        public decimal DefaultPricePerSlot { get; set; }

        public int? Parent7aSideId { get; set; }
        public List<int> Child5aSideIds { get; set; } = new List<int>();

        public List<CreatePricingRuleDto> PricingRules { get; set; } = new List<CreatePricingRuleDto>();
    }
}