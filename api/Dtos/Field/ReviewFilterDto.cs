using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ReviewFilterDto
    {
        /// <summary>
        /// Số trang hiện tại (mặc định: 1).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số lượng mục trên mỗi trang (mặc định: 10).
        /// </summary>
        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Điểm đánh giá tối thiểu để lọc các đánh giá.
        /// </summary>
        [Range(1, 5, ErrorMessage = "Điểm đánh giá tối thiểu phải từ 1 đến 5.")]
        public int? MinRating { get; set; }

        /// <summary>
        /// Trường để sắp xếp (createdAt, rating).
        /// </summary>
        [RegularExpression("^(createdAt|rating)$", ErrorMessage = "Trường sắp xếp phải là 'createdAt' hoặc 'rating'.")]
        public string SortBy { get; set; } = "createdAt";

        /// <summary>
        /// Thứ tự sắp xếp (asc/desc).
        /// </summary>
        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "desc";
    }
}