using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO để lọc thời gian trống.
    /// </summary>
    public class AvailabilityFilterDto
    {
        [Required(ErrorMessage = "Ngày là bắt buộc.")]
        public DateTime Date { get; set; }

        public int? SubFieldId { get; set; }
        public int? SportId { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
}