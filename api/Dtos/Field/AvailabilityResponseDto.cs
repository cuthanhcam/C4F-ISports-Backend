using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class AvailabilityResponseDto
    {
        public int SubFieldId { get; set; }

        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh sách khung giờ trống là bắt buộc.")]
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    }
}