using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin tiện ích sân.
    /// </summary>
    public class FieldAmenityDto
    {
        public int FieldId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }
}