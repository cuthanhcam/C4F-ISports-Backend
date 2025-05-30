using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class BookingFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải là số nguyên dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Trạng thái phải là 'Pending', 'Confirmed' hoặc 'Cancelled'.")]
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}