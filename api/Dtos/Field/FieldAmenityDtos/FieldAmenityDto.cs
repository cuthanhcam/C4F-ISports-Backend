using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldAmenityDtos
{
    public class FieldAmenityDto
    {
        public int FieldAmenityId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
    }
}