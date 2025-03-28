using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldDescription
    {
        public int FieldDescriptionId { get; set; }
        public int FieldId { get; set; }
        public string Description { get; set; } // Mô tả sân (vd: Sân cỏ nhân tạo, Sân bóng đá mini,...)

        public Field Field { get; set; }
    }
}