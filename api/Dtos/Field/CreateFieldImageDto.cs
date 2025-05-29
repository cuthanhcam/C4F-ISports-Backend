using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class CreateFieldImageDto
    {
        /// <summary>
        /// URL của hình ảnh.
        /// </summary>
        [Required(ErrorMessage = "URL hình ảnh là bắt buộc.")]
        [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh phải là một URL hợp lệ.")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// ID công khai của hình ảnh (ví dụ: Cloudinary).
        /// </summary>
        [StringLength(500, ErrorMessage = "ID công khai không được vượt quá 500 ký tự.")]
        public string? PublicId { get; set; }

        /// <summary>
        /// URL của hình ảnh thu nhỏ.
        /// </summary>
        [StringLength(500, ErrorMessage = "URL hình ảnh thu nhỏ không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "URL hình ảnh thu nhỏ phải là một URL hợp lệ.")]
        public string? Thumbnail { get; set; }

        /// <summary>
        /// Cho biết đây có phải là hình ảnh chính hay không.
        /// </summary>
        public bool IsPrimary { get; set; }
    }
}