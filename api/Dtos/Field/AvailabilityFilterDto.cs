using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class AvailabilityFilterDto
    {
        /// <summary>
        /// Ngày để kiểm tra tình trạng trống.
        /// </summary>
        [Required(ErrorMessage = "Ngày là bắt buộc.")]
        public DateTime Date { get; set; }

        /// <summary>
        /// ID sân con để lọc tình trạng trống.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ID sân con phải là số nguyên dương.")]
        public int? SubFieldId { get; set; }

        /// <summary>
        /// ID môn thể thao để lọc tình trạng trống.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ID môn thể thao phải là số nguyên dương.")]
        public int? SportId { get; set; }

        /// <summary>
        /// Thời gian bắt đầu để kiểm tra tình trạng trống (HH:mm).
        /// </summary>
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian bắt đầu phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc để kiểm tra tình trạng trống (HH:mm).
        /// </summary>
        [RegularExpression(@"^([0-1][0-9]|2[0-3]):[0|3]0$", ErrorMessage = "Thời gian kết thúc phải theo định dạng HH:mm và là bội số của 30 phút.")]
        public string? EndTime { get; set; }
    }
}