using System;

namespace api.Dtos.User
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string BookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string CreatedAt { get; set; }
    }
} 