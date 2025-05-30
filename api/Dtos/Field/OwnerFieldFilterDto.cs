using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class OwnerFieldFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Số lượng phần tử mỗi trang phải từ 1 đến 50.")]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public string? Status { get; set; } // Active, Inactive

        public int? SportId { get; set; }

        public string? SortBy { get; set; } = "createdAt"; // fieldName, createdAt, rating, bookingCount

        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }
}