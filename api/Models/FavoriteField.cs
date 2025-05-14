using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class FavoriteField
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }

        public required User User { get; set; }
        public required Field Field { get; set; }
    }
}