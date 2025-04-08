using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Sport
    {
        public int SportId { get; set; } // Mã loại thể thao (PK)
        public string SportName { get; set; } // Tên loại thể thao (VD: Bóng đá, Cầu lông)
        
        public ICollection<Field> Fields { get; set; }
    }
}