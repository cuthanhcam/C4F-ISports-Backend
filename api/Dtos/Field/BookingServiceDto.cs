using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin dịch vụ đặt sân.
    /// </summary>
    public class BookingServiceDto
    {
        public int BookingServiceId { get; set; }
        public int FieldServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}