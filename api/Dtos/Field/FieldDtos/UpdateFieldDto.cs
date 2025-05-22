using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    public class UpdateFieldDto
    {
        [Required, StringLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [Required, StringLength(100), RegularExpression(@"^\d{2}:\d{2}-\d{2}:\d{2}$", ErrorMessage = "OpenHours must be in format HH:mm-HH:mm")]
        public string OpenHours { get; set; } = string.Empty;

        [Required, StringLength(20), RegularExpression("^(Active|Inactive|Maintenance)$")]
        public string Status { get; set; } = "Active";

        [Required]
        public int SportId { get; set; }
    }
}