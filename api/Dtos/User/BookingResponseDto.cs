using System;

namespace api.Dtos.User
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string CreatedAt { get; set; }
    }
} 