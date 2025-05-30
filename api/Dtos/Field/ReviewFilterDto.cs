using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class ReviewFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [Range(1, 5, ErrorMessage = "Điểm đánh giá tối thiểu phải từ 1 đến 5.")]
        public int? MinRating { get; set; }

        [RegularExpression("^(createdAt|rating)$", ErrorMessage = "Trường sắp xếp phải là 'createdAt' hoặc 'rating'.")]
        public string SortBy { get; set; } = "createdAt";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "desc";
    }
}