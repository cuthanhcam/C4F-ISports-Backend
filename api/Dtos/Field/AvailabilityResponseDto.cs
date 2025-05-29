using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class AvailabilityResponseDto
    {
        /// <summary>
        /// Mã định danh duy nhất của sân con.
        /// </summary>
        public int SubFieldId { get; set; }

        /// <summary>
        /// Tên của sân con.
        /// </summary>
        [Required(ErrorMessage = "Tên sân con là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sân con không được vượt quá 100 ký tự.")]
        public string SubFieldName { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách các khung giờ trống.
        /// </summary>
        [Required(ErrorMessage = "Danh sách khung giờ trống là bắt buộc.")]
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    }
}