using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Models;
using Microsoft.AspNetCore.Http;

namespace api.Interfaces
{
    public interface IFieldService
    {
        Task<(List<FieldResponseDto> Data, int Total, int Page, int PageSize)> GetFilteredFieldsAsync(FieldFilterDto filterDto);
        Task<FieldResponseDto> GetFieldByIdAsync(int fieldId, string? include);
        Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto validateAddressDto);
        Task<FieldResponseDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto);
        Task<FieldImageResponseDto> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, IFormFile image, bool isPrimary);
        Task<FieldResponseDto> UpdateFieldAsync(ClaimsPrincipal user, int fieldId, UpdateFieldDto updateFieldDto);
        Task DeleteFieldAsync(ClaimsPrincipal user, int fieldId);
        Task<List<AvailabilityResponseDto>> GetFieldAvailabilityAsync(int fieldId, AvailabilityFilterDto filterDto);
        Task<(List<ReviewResponseDto> Data, int Total, int Page, int PageSize)> GetFieldReviewsAsync(int fieldId, ReviewFilterDto filterDto);
        Task<(List<BookingResponseDto> Data, int Total, int Page, int PageSize)> GetFieldBookingsAsync(ClaimsPrincipal user, int fieldId, BookingFilterDto filterDto);
    }
}