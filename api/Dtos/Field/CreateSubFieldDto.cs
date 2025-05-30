using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateSubFieldDto
    {
        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại sân là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Loại sân không được vượt quá 50 ký tự.")]
        [RegularExpression("^(5-a-side|7-a-side|11-a-side|Badminton)$", ErrorMessage = "Loại sân phải là '5-a-side', '7-a-side', '11-a-side' hoặc 'Badminton'.")]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số nguyên dương.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(CreateSubFieldDto), nameof(ValidateSubFieldTime), ErrorMessage = "Thời gian hoạt động của sân con phải nằm trong thời gian hoạt động của sân chính.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mặc định mỗi khung giờ là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mặc định mỗi khung giờ phải là số không âm.")]
        public decimal DefaultPricePerSlot { get; set; }

        [MaxLength(20, ErrorMessage = "Tối đa 20 quy tắc giá được phép.")]
        public List<CreatePricingRuleDto> PricingRules { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "ID sân 7-a-side cha phải là số nguyên dương.")]
        public int? Parent7aSideId { get; set; }

        [MaxLength(3, ErrorMessage = "Tối đa 3 sân 5-a-side con được phép.")]
        public List<int> Child5aSideIds { get; set; } = new();

        public static ValidationResult ValidateSubFieldTime(string openTime, ValidationContext context)
        {
            var instance = (CreateSubFieldDto)context.ObjectInstance;
            var fieldDto = context.ObjectInstance as CreateFieldDto ?? (context.ObjectInstance as UpdateFieldDto);
            if (fieldDto != null && TimeSpan.TryParse(fieldDto.OpenTime, out var fieldOpenTime) &&
                TimeSpan.TryParse(fieldDto.CloseTime, out var fieldCloseTime) &&
                TimeSpan.TryParse(openTime, out var subFieldOpenTime) &&
                TimeSpan.TryParse(instance.CloseTime, out var subFieldCloseTime))
            {
                if (subFieldOpenTime < fieldOpenTime || subFieldCloseTime > fieldCloseTime)
                    return new ValidationResult("Thời gian hoạt động của sân con phải nằm trong thời gian hoạt động của sân chính.");
            }
            return ValidationResult.Success;
        }
    }
}