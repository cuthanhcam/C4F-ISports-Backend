using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldAmenity
    {
        public int FieldAmenityId { get; set; }
        public int FieldId { get; set; }
        public string AmenityName { get; set; } // Tên tiện ích (vd: Wifi, Điều hòa, Nước uống,...)

        public Field Field { get; set; }
    }
}
