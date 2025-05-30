using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Promotion
{
    public class ApplyPromotionResponseDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int PromotionId { get; set; }

        [Required]
        public decimal Discount { get; set; }

        [Required]
        public decimal NewTotalPrice { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}