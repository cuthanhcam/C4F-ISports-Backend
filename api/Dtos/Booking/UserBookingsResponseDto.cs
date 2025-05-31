using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class UserBookingsResponseDto
    {
        public List<BookingSummaryDto> Data { get; set; } = new List<BookingSummaryDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}