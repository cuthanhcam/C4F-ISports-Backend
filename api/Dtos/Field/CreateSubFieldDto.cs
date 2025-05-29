using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateSubFieldDto
    {
        /// <summary>
        /// Tên của sân con.
        /// </summary>
        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        /// <summary>
        /// Loại sân con (ví dụ: 5-a-side, 7-a-side).
        /// </summary>
        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        [RegularExpression("^(5-a-side|7-a-side|11-a-side|Badminton)$", ErrorMessage = "Loại sân phải là '5-a-side', '7-a-side', '11-a-side' hoặc 'Badminton'.")]
        public string FieldType { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả của sân con.
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        /// <summary>
        /// Sức chứa tối đa của sân con.
        /// </summary>
        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số nguyên dương.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Thời gian mở cửa của sân con (HH:mm).
        /// </summary>
        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian đóng cửa của sân con (HH:mm).
        /// </summary>
        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        /// <summary>
        /// Giá mặc định mỗi khung giờ.
        /// </summary>
        [Required(ErrorMessage = "Giá mặc định mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mặc định mỗi khung giờ phải là số không âm.")]
        public decimal DefaultPricePerSlot { get; set; }

        /// <summary>
        /// Danh sách các quy tắc giá cho sân con.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Tối đa 20 quy tắc giá được phép.")]
        public List<CreatePricingRuleDto> PricingRules { get; set; } = new();

        /// <summary>
        /// ID của sân 7-a-side cha (nếu có).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ID sân 7-a-side cha phải là số nguyên dương.")]
        public int? Parent7aSideId { get; set; }

        /// <summary>
        /// Danh sách các ID sân 5-a-side con (nếu có).
        /// </summary>
        [MaxLength(3, ErrorMessage = "Tối đa 3 sân 5-a-side con được phép.")]
        public List<int> Child5aSideIds { get; set; } = new();
    }
}