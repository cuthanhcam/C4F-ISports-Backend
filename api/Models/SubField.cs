using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class SubField
    {
        public int SubFieldId { get; set; }
        public int FieldId { get; set; }
        public string SubFieldName { get; set; }
        public string Size { get; set; } // Đổi thành string để lưu "Badminton"
        public decimal PricePerHour { get; set; }
        public string Status { get; set; } // Thêm Status

        public Field Field { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}