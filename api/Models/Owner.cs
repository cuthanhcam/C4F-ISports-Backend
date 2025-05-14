using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Owner
    {
        public int OwnerId { get; set; }
        public int AccountId { get; set; }

        [Required, StringLength(100)]
        public required string FullName { get; set; }

        [Required, StringLength(20)]
        public required string Phone { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public required Account Account { get; set; }
        public ICollection<Field> Fields { get; set; } = new List<Field>();
    }
}