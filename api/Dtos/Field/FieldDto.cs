using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field
{
    /// <summary>
    /// DTO chứa thông tin sân thể thao dùng cho danh sách.
    /// </summary>
    public class FieldDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public int SportId { get; set; }
        public double? Distance { get; set; }
        public decimal? MinPricePerSlot { get; set; }
        public decimal? MaxPricePerSlot { get; set; }
    }
}