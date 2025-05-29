using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class BookingFilterDto
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
        /// Trạng thái đặt sân để lọc.
        /// </summary>
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự.")]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Trạng thái phải là 'Pending', 'Confirmed' hoặc 'Cancelled'.")]
        public string? Status { get; set; }

        /// <summary>
        /// Ngày bắt đầu để lọc các đặt sân.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc để lọc các đặt sân.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}