using System.Collections.Generic;

namespace api.Dtos.Booking
{
    public class BookingPreviewDto
    {
        public int SubFieldId { get; set; }
        public string FieldName { get; set; }
        public string SubFieldName { get; set; }
        public string BookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public List<BookingServiceDto> Services { get; set; }
    }
}