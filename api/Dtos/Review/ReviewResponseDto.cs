using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Review
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int FieldId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}