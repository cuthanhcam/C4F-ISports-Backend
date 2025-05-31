using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Payment
{
    public class ProcessRefundRequestDto
    {
        [Required, StringLength(20), RegularExpression("^(Approved|Rejected)$")]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Note { get; set; }
    }
}