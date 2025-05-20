using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class UserReviewDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ReviewId phải là số dương.")]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân không được vượt quá 100 ký tự.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Bình luận là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày tạo là bắt buộc.")]
        public DateTime CreatedAt { get; set; }
    }
}