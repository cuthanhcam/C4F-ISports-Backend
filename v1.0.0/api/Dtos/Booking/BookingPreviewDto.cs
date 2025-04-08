using System.Collections.Generic;

namespace api.Dtos.Booking
{
    public class BookingPreviewDto
    {
        public string FieldName { get; set; } // Tên field chung
        public string BookingDate { get; set; }
        public List<SubFieldPreviewDto> SubFields { get; set; } // Danh sách subfields
        public double TotalHours { get; set; } // Tổng số giờ của tất cả subfields
        public decimal TotalPrice { get; set; } // Tổng giá của tất cả subfields và dịch vụ
        public List<BookingServiceDto> Services { get; set; } // Dịch vụ đi kèm
    }

    public class SubFieldPreviewDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public List<TimeSlotPreviewDto> TimeSlots { get; set; }
        public double SubFieldTotalHours { get; set; } // Tổng giờ của subfield này
        public decimal SubFieldTotalPrice { get; set; } // Tổng giá của subfield này
    }

    public class TimeSlotPreviewDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public double Duration { get; set; } // Số giờ
        public decimal SlotPrice { get; set; }
    }
}