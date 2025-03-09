using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class FieldImage
    {
        public int FieldImageId { get; set; }
        public int FieldId { get; set; }
        public string? Thumbnail { get; set; }
        public string ImageUrl { get; set; }
        
        public Field Field { get; set; }
    }
}