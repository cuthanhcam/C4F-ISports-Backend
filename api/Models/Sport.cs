using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Sport
    {
        public int SportId { get; set; }

        [Required, StringLength(50)]
        public string SportName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? IconUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }

}