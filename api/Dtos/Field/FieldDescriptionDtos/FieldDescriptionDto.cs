using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.FieldDescriptionDtos
{
    public class FieldDescriptionDto
    {
        public int FieldDescriptionId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}