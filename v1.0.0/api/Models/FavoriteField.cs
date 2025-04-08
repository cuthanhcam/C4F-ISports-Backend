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
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Field Field { get; set; }
    }
}