using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FieldDescription
    {
        public int FieldDescriptionId { get; set; }
        public int FieldId { get; set; }

        [Required, StringLength(2000)]
        public required string Description { get; set; }

        public required Field Field { get; set; }
    }
}