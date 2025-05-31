using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class BookingServicesResponseDto
    {
        public List<BookingServiceResponseDto> Data { get; set; } = new List<BookingServiceResponseDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}