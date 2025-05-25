using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(SearchDateTime))]
    public class SearchHistory
    {
        [Key]
        public int SearchId { get; set; } 

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Keyword { get; set; }

        [Required]
        public DateTime SearchDateTime { get; set; }

        public int? FieldId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTime? DeletedAt { get; set; }

        [ForeignKey("UserId")]
        public User Account { get; set; }

        [ForeignKey("FieldId")]
        public Field? Field { get; set; }

        public SearchHistory()
        {
            SearchDateTime = DateTime.UtcNow;
        }
    }
}