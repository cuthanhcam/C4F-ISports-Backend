using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Payment
{
    public class CreatePaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int MainBookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}