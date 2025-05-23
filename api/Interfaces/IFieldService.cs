using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Dtos.Field.AddressValidationDtos;
using api.Dtos.Field.FieldAmenityDtos;
using api.Dtos.Field.FieldAvailabilityDtos;
using api.Dtos.Field.FieldDescriptionDtos;
using api.Dtos.Field.FieldImageDtos;
using api.Dtos.Field.FieldPricingDtos;
using api.Dtos.Field.FieldServiceDtos;
using Microsoft.AspNetCore.Http;

namespace api.Interfaces
{
    public interface IFieldService
    {
        Task<FieldDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto);
        Task<FieldDto> UpdateFieldAsync(ClaimsPrincipal user, int fieldId, UpdateFieldDto updateFieldDto);
        Task DeleteFieldAsync(ClaimsPrincipal user, int fieldId);
        Task<FieldDto> GetFieldByIdAsync(int fieldId);
        Task<IEnumerable<FieldDto>> GetAllFieldsAsync();
        Task<FieldImageDto> AddFieldImageAsync(ClaimsPrincipal user, int fieldId, IFormFile image);
        Task DeleteFieldImageAsync(ClaimsPrincipal user, int fieldId, int imageId);
        Task<FieldServiceDto> CreateFieldServiceAsync(ClaimsPrincipal user, int fieldId, CreateFieldServiceDto createFieldServiceDto);
        Task<FieldServiceDto> UpdateFieldServiceAsync(ClaimsPrincipal user, int fieldId, int serviceId, UpdateFieldServiceDto updateFieldServiceDto);
        Task<FieldAmenityDto> CreateFieldAmenityAsync(ClaimsPrincipal user, int fieldId, CreateFieldAmenityDto createFieldAmenityDto);
        Task<FieldAmenityDto> UpdateFieldAmenityAsync(ClaimsPrincipal user, int fieldId, int amenityId, UpdateFieldAmenityDto updateFieldAmenityDto);
        Task<FieldDescriptionDto> CreateFieldDescriptionAsync(ClaimsPrincipal user, int fieldId, CreateFieldDescriptionDto createFieldDescriptionDto);
        Task<FieldDescriptionDto> UpdateFieldDescriptionAsync(ClaimsPrincipal user, int fieldId, int descriptionId, UpdateFieldDescriptionDto updateFieldDescriptionDto);
        Task<FieldPricingDto> CreateFieldPricingAsync(ClaimsPrincipal user, int fieldId, CreateFieldPricingDto createFieldPricingDto);
        Task<FieldPricingDto> UpdateFieldPricingAsync(ClaimsPrincipal user, int fieldId, int pricingId, UpdateFieldPricingDto updateFieldPricingDto);
        Task<IEnumerable<FieldAvailabilityDto>> GetFieldAvailabilityAsync(int? fieldId, DateTime? date);
        Task<AddressValidationResultDto> ValidateAddressAsync(ValidateAddressDto addressDto);
    }
}