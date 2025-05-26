using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class LoyaltyPointsDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "UserId phải là số dương.")]
        public int UserId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Điểm loyalty phải là số không âm.")]
        public decimal LoyaltyPoints { get; set; }
    }
}