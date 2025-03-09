using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public int FieldId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }

        public Field Field { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } // Thêm quan hệ 1-n
    }
}