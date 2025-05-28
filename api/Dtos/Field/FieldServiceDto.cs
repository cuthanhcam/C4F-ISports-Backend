using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin dịch vụ sân.
    /// </summary>
    public class FieldServiceDto
    {
        public int FieldServiceId { get; set; }
        public string ServiceName { get; set ; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}