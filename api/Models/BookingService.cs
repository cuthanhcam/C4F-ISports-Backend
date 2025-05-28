using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class BookingService
    {
        public int BookingServiceId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int FieldServiceId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;

        [ForeignKey("FieldServiceId")]
        public FieldService FieldService { get; set; } = null!;
    }
}