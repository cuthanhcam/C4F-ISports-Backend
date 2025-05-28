using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để lọc danh sách đặt sân.
    /// </summary>
    public class BookingFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page phải là số dương.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;

        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}