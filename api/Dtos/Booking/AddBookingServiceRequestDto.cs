using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Booking
{
    public class AddBookingServiceRequestDto
    {
        [Required(ErrorMessage = "FieldServiceId là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "FieldServiceId phải là số dương.")]
        public int FieldServiceId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là số dương.")]
        public int Quantity { get; set; }
    }
}