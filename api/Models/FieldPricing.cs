using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldPricing
    {
        public int FieldPricingId { get; set; }
        public int FieldId { get; set; }
        public TimeSpan StartTime { get; set; } // Giờ bắt đầu (vd: 17:00)
        public TimeSpan EndTime { get; set; } // Giờ kết thúc (vd: 19:00)
        public int DayOfWeek { get; set; } // Ngày trong tuần (0: Chủ nhật, 1: Thứ 2, ..., 6: Thứ 7)
        public decimal Price { get; set; } // Giá sân/1h (vd: 200000)

        public Field Field { get; set; }
    }
}