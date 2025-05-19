using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class SearchHistory
    {
        public int SearchHistoryId { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(500)]
        public required string SearchQuery { get; set; }

        [Required]
        public DateTime SearchDate { get; set; }

        public int? FieldId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public User User { get; set; }
        public Field? Field { get; set; }
    }
}