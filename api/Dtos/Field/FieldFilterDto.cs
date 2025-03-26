using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldFilterDto
    {
        public int? SportId { get; set; }

        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự")]
        public string Location { get; set; }

        [RegularExpression("^(Active|Inactive|Maintenance)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; }

        [RegularExpression(@"^(FieldName|Location|Rating|Price):(asc|desc)$", ErrorMessage = "Sắp xếp không hợp lệ")]
        public string Sort { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
        public int PageSize { get; set; } = 10;
    }
} 