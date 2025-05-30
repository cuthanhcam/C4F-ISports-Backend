using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string? District { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        [StringLength(200, ErrorMessage = "Từ khóa tìm kiếm không được vượt quá 200 ký tự.")]
        public string? Search { get; set; }

        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double? Longitude { get; set; }

        [Range(0, 1000, ErrorMessage = "Bán kính phải từ 0 đến 1000 km.")]
        public double Radius { get; set; } = 10;

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải là số không âm.")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa phải là số không âm.")]
        public decimal? MaxPrice { get; set; }

        [RegularExpression("^(fieldId|fieldName|averageRating|distance|price)$", ErrorMessage = "Trường sắp xếp phải là 'fieldId', 'fieldName', 'averageRating', 'distance' hoặc 'price'.")]
        public string SortBy { get; set; } = "fieldId";

        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "asc";
    }
}