using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin chi tiết của sân thể thao.
    /// </summary>
    public class FieldDetailsDto : FieldDto
    {
        public List<SubFieldDto> SubFields { get; set; } = new List<SubFieldDto>();
        public List<FieldServiceDto> Services { get; set; } = new List<FieldServiceDto>();
        public List<FieldAmenityDto> Amenities { get; set; } = new List<FieldAmenityDto>();
        public List<FieldImageDto> Images { get; set; } = new List<FieldImageDto>();
    }
}