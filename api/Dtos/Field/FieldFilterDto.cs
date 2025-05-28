using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để lọc danh sách sân thể thao.
    /// </summary>
    public class FieldFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page phải là số dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        public string? City { get; set; }
        public string? District { get; set; }
        public int? SportId { get; set; }
        public string? Search { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Range(0, 100, ErrorMessage = "Radius phải từ 0 đến 100 km.")]
        public double Radius { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } = "fieldId";
        public string? SortOrder { get; set; } = "asc";
    }
}