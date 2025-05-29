using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class FieldFilterDto
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
        /// Thành phố để lọc các sân.
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự.")]
        public string? City { get; set; }

        /// <summary>
        /// Quận/Huyện để lọc các sân.
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên quận/huyện không được vượt quá 100 ký tự.")]
        public string? District { get; set; }

        /// <summary>
        /// ID môn thể thao để lọc các sân.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        /// <summary>
        /// Từ khóa tìm kiếm cho tên sân hoặc địa chỉ.
        /// </summary>
        [StringLength(200, ErrorMessage = "Từ khóa tìm kiếm không được vượt quá 200 ký tự.")]
        public string? Search { get; set; }

        /// <summary>
        /// Vĩ độ để lọc dựa trên khoảng cách.
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Kinh độ để lọc dựa trên khoảng cách.
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Bán kính tìm kiếm tính bằng kilômét (mặc định: 10).
        /// </summary>
        [Range(0, 1000, ErrorMessage = "Bán kính phải từ 0 đến 1000 km.")]
        public double Radius { get; set; } = 10;

        /// <summary>
        /// Giá tối thiểu mỗi khung giờ.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải là số không âm.")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Giá tối đa mỗi khung giờ.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa phải là số không âm.")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Trường để sắp xếp (fieldId, fieldName, averageRating, distance, price).
        /// </summary>
        [RegularExpression("^(fieldId|fieldName|averageRating|distance|price)$", ErrorMessage = "Trường sắp xếp phải là 'fieldId', 'fieldName', 'averageRating', 'distance' hoặc 'price'.")]
        public string SortBy { get; set; } = "fieldId";

        /// <summary>
        /// Thứ tự sắp xếp (asc/desc).
        /// </summary>
        [RegularExpression("^(asc|desc)$", ErrorMessage = "Thứ tự sắp xếp phải là 'asc' hoặc 'desc'.")]
        public string SortOrder { get; set; } = "asc";
    }
}