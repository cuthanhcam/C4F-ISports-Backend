using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.SubFieldDtos
{
    public class CreateSubFieldDto
    {
        [Required, StringLength(100)]
        public string SubFieldName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string FieldType { get; set; } = string.Empty;

        [StringLength(20), RegularExpression("^(Active|Inactive)$")]
        public string? Status { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}