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
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string OpenHours { get; set; } = string.Empty;
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public string Status { get; set; } = "Active";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? AverageRating { get; set; }
        public int SportId { get; set; }
        public List<SubFieldDto> SubFields { get; set; } = new List<SubFieldDto>();
        public List<FieldImageDto> FieldImages { get; set; } = new List<FieldImageDto>();
        public List<FieldServiceDto> FieldServices { get; set; } = new List<FieldServiceDto>();
        public List<FieldAmenityDto> FieldAmenities { get; set; } = new List<FieldAmenityDto>();
        public List<FieldDescriptionDto> FieldDescriptions { get; set; } = new List<FieldDescriptionDto>();
        public List<FieldPricingDto> FieldPricings { get; set; } = new List<FieldPricingDto>();
    }
}