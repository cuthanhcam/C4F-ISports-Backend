using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin thời gian trống của sân con.
    /// </summary>
    public class SubFieldAvailabilityDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public List<TimeSlotAvailabilityDto> AvailableSlots { get; set; } = new List<TimeSlotAvailabilityDto>();
    }
}