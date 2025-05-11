using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
     public class FavoriteField
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }

        public User User { get; set; }
        public Field Field { get; set; }
    }
}