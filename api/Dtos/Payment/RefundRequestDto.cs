using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Payment
{
    public class RefundRequestDto
    {
        [Required, Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}