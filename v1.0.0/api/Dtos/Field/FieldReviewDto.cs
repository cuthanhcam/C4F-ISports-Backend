using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldReviewDto
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Đánh giá không được để trống")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống")]
        [StringLength(500, ErrorMessage = "Nội dung đánh giá không được vượt quá 500 ký tự")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CreateFieldReviewDto
    {
        [Required(ErrorMessage = "Đánh giá không được để trống")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống")]
        [StringLength(500, ErrorMessage = "Nội dung đánh giá không được vượt quá 500 ký tự")]
        public string Comment { get; set; }
    }
}