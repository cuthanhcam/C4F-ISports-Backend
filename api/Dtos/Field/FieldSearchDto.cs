using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldSearchDto
    {
        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự")]
        public string Location { get; set; }
        public int? SportId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Time { get; set; }
        [Range(-90, 90, ErrorMessage = "Vĩ độ không hợp lệ")]
        public decimal? Latitude { get; set; }
        [Range(-180, 180, ErrorMessage = "Kinh độ không hợp lệ")]
        public decimal? Longitude { get; set; }
        [Range(0, 100, ErrorMessage = "Bán kính tìm kiếm phải từ 0 đến 100 km")]
        public decimal? Radius { get; set; }
        [RegularExpression(@"^(Distance|Rating|Price):(asc|desc)$", ErrorMessage = "Sắp xếp không hợp lệ")]
        public string Sort { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
        public int Page { get; set; } = 1;
        [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
        public int PageSize { get; set; } = 10;
    }
} 