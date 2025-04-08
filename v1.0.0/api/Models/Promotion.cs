using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MinBookingValue { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool IsActive { get; set; }
    }
}