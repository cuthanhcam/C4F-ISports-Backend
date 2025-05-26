using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class SearchHistoryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "SearchId phải là số dương.")]
        public int SearchId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "UserId phải là số dương.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Từ khóa là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Từ khóa không được vượt quá 500 ký tự.")]
        public string Keyword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày tìm kiếm là bắt buộc.")]
        public DateTime SearchDateTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "FieldId phải là số dương.")]
        public int? FieldId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}