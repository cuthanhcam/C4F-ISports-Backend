using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.SubFieldDtos
{
    public class SubFieldDto
    {
        public int SubFieldId { get; set; }
        public string SubFieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}