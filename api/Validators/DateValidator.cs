using System;
using System.ComponentModel.DataAnnotations;

namespace api.Validators
{
    public class DateValidator
    {
        public static ValidationResult? ValidateBirthDate(DateTime? birthDate, ValidationContext context)
        {
            if (birthDate == null)
                return ValidationResult.Success;

            var date = birthDate.Value;

            // Kiểm tra ngày sinh không ở tương lai
            if (date > DateTime.Today)
                return new ValidationResult("Ngày sinh không thể ở tương lai.");

            // Kiểm tra tuổi hợp lệ (ví dụ: không quá 120 tuổi)
            if (DateTime.Today.Year - date.Year > 120)
                return new ValidationResult("Ngày sinh không hợp lệ.");

            return ValidationResult.Success;
        }
    }
}