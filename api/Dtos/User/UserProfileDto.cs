using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class UserProfileDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "UserId phải là số dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [RegularExpression(@"^[\p{L}\p{M}\s'.-]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái, khoảng trắng, dấu chấm hoặc dấu gạch ngang.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ. Phải có 10 chữ số và bắt đầu bằng 03, 05, 07, 08 hoặc 09.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Thành phố không được vượt quá 100 ký tự.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự.")]
        public string? District { get; set; }

        [StringLength(500, ErrorMessage = "URL ảnh đại diện không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL ảnh đại diện không hợp lệ.")]
        public string? AvatarUrl { get; set; }
    }
}