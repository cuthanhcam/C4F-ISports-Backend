using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } // "CreditCard", "BankTransfer", "Cash"

        [Required, StringLength(100)]
        public string TransactionId { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } // "Success", "Pending", "Failed"

        [Required, StringLength(3)]
        public string Currency { get; set; } = "VND";

        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }

        public Booking Booking { get; set; }
    }
}