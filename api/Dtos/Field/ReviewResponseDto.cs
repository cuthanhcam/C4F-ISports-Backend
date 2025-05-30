using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "ID người dùng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID người dùng phải là số nguyên dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Bình luận là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày tạo là bắt buộc.")]
        public DateTime CreatedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Phản hồi của chủ sân không được vượt quá 1000 ký tự.")]
        public string? OwnerReply { get; set; }

        public DateTime? ReplyDate { get; set; }
    }
}