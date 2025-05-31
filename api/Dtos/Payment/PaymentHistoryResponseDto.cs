using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Payment
{
    public class PaymentHistoryResponseDto
    {
        public List<PaymentSummaryDto> Data { get; set; } = new List<PaymentSummaryDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}