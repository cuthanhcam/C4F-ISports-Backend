using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class OwnerFieldResponseDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public int SubFieldCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string PrimaryImage { get; set; } = string.Empty;
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
    
    }
}