using System;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Dtos;
using api.Models;
using System.Security.Claims;

namespace api.Interfaces
{
    public interface IFieldService
    {
        Task<PaginatedResponse<FieldDto>> GetFieldsAsync(FieldFilterDto filter);
        Task<FieldDto> GetFieldByIdAsync(int id);
        Task<PaginatedResponse<FieldDto>> SearchFieldsAsync(FieldSearchDto search);
        Task<FieldDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto);
        Task<FieldDto> UpdateFieldAsync(ClaimsPrincipal user, int id, UpdateFieldDto updateFieldDto);
        Task DeleteFieldAsync(ClaimsPrincipal user, int id);
        Task<PaginatedResponse<FieldReviewDto>> GetFieldReviewsAsync(int fieldId, int? rating, string sort, int page, int pageSize);
        Task<PaginatedResponse<FieldDto>> GetNearbyFieldsAsync(decimal latitude, decimal longitude, decimal radius, string sort, int page, int pageSize);
        Task<string> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, string imageBase64);
        Task<PaginatedResponse<FieldDto>> GetOwnerFieldsAsync(ClaimsPrincipal user, string status, string sort, int page, int pageSize);
        Task ReportFieldAsync(int fieldId, FieldReportDto reportDto);
        Task<PaginatedResponse<FieldDto>> GetSuggestedFieldsAsync(decimal? latitude, decimal? longitude, int page, int pageSize);
    }
} 