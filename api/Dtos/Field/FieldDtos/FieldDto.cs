using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Field.FieldAmenityDtos;
using api.Dtos.Field.FieldDescriptionDtos;
using api.Dtos.Field.FieldImageDtos;
using api.Dtos.Field.FieldPricingDtos;
using api.Dtos.Field.FieldServiceDtos;
using api.Dtos.Field.SubFieldDtos;
using api.Models;

namespace api.Dtos.Field
{
    public class FieldDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public decimal? AverageRating { get; set; }
        public int SportId { get; set; }
        public List<SubFieldDto> SubFields { get; set; } = new List<SubFieldDto>();
        public List<FieldImageDto> Images { get; set; } = new List<FieldImageDto>();
        public List<FieldServiceDto> Services { get; set; } = new List<FieldServiceDto>();
        public List<FieldAmenityDto> Amenities { get; set; } = new List<FieldAmenityDto>();
        public List<FieldDescriptionDto> Descriptions { get; set; } = new List<FieldDescriptionDto>();
        public List<FieldPricingDto> Pricing { get; set; } = new List<FieldPricingDto>();
    }
}