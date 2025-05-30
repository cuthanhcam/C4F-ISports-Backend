using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class UpdateFieldDto
    {
        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian mở cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian mở cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string OpenTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian đóng cửa là bắt buộc.")]
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian đóng cửa phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(UpdateFieldDto), nameof(ValidateCloseTime), ErrorMessage = "Thời gian đóng cửa phải sau thời gian mở cửa.")]
        public string CloseTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID môn thể thao là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int SportId { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một sân con.")]
        [MinLength(1, ErrorMessage = "Phải cung cấp ít nhất một sân con.")]
        [MaxLength(10, ErrorMessage = "Tối đa 10 sân con được phép.")]
        public List<UpdateSubFieldDto> SubFields { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 dịch vụ được phép.")]
        public List<UpdateFieldServiceDto> Services { get; set; } = new();

        [MaxLength(50, ErrorMessage = "Tối đa 50 tiện ích được phép.")]
        public List<UpdateFieldAmenityDto> Amenities { get; set; } = new();

        [MaxLength(20, ErrorMessage = "Tối đa 20 hình ảnh được phép.")]
        public List<IFormFile> Images { get; set; } = new();

        public static ValidationResult ValidateCloseTime(string closeTime, ValidationContext context)
        {
            var instance = (UpdateFieldDto)context.ObjectInstance;
            if (TimeSpan.TryParse(instance.OpenTime, out var openTime) && TimeSpan.TryParse(closeTime, out var closeTimeSpan))
            {
                if (closeTimeSpan <= openTime)
                    return new ValidationResult("Thời gian đóng cửa phải sau thời gian mở cửa.");
            }
            return ValidationResult.Success;
        }
    }
}