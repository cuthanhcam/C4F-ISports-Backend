using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để lọc danh sách đánh giá.
    /// </summary>
    public class ReviewFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page phải là số dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [Range(1, 5, ErrorMessage = "Điểm tối thiểu phải từ 1 đến 5.")]
        public int? MinRating { get; set; }
        public string? SortBy { get; set; } = "createdAt";
        public string? SortOrder { get; set; } = "desc";
    }
}