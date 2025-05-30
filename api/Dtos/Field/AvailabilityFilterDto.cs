using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class AvailabilityFilterDto
    {
        [Required(ErrorMessage = "Ngày là bắt buộc.")]
        public DateTime Date { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID sân con phải là số nguyên dương.")]
        public int? SubFieldId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string? StartTime { get; set; }

        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        [CustomValidation(typeof(AvailabilityFilterDto), nameof(ValidateTimeRange), ErrorMessage = "Thời gian kết thúc phải sau thời gian bắt đầu.")]
        public string? EndTime { get; set; }

        public static ValidationResult ValidateTimeRange(string endTime, ValidationContext context)
        {
            var instance = (AvailabilityFilterDto)context.ObjectInstance;
            if (!string.IsNullOrEmpty(instance.StartTime) && !string.IsNullOrEmpty(endTime) &&
                TimeSpan.TryParse(instance.StartTime, out var startTime) &&
                TimeSpan.TryParse(endTime, out var endTimeSpan))
            {
                if (endTimeSpan <= startTime)
                    return new ValidationResult("Thời gian kết thúc phải sau thời gian bắt đầu.");
            }
            return ValidationResult.Success;
        }
    }
}