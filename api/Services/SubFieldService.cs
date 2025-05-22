using System;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Field.SubFieldDtos;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class SubFieldService : ISubFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubFieldService> _logger;

        public SubFieldService(IUnitOfWork unitOfWork, ILogger<SubFieldService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SubFieldDto> CreateSubFieldAsync(ClaimsPrincipal user, int fieldId, CreateSubFieldDto createSubFieldDto)
        {
            var ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                _logger.LogError("User ID not found in claims");
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
            }

            var field = await _unitOfWork.Repository<Field>().GetByIdAsync(fieldId);
            if (field == null || field.OwnerId != int.Parse(ownerId))
            {
                _logger.LogWarning("Field not found or user not authorized: FieldId={FieldId}, OwnerId={OwnerId}", fieldId, ownerId);
                throw new UnauthorizedAccessException("Bạn không có quyền thêm sân nhỏ cho sân này.");
            }

            var subField = new SubField
            {
                FieldId = fieldId,
                SubFieldName = createSubFieldDto.SubFieldName,
                FieldType = createSubFieldDto.FieldType,
                Status = createSubFieldDto.Status ?? "Active",
                Capacity = createSubFieldDto.Capacity,
                Description = createSubFieldDto.Description
            };

            await _unitOfWork.Repository<SubField>().AddAsync(subField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("SubField created for field: {FieldId}, SubFieldId={SubFieldId}", fieldId, subField.SubFieldId);
            return new SubFieldDto
            {
                SubFieldId = subField.SubFieldId,
                SubFieldName = subField.SubFieldName,
                FieldType = subField.FieldType,
                Status = subField.Status,
                Capacity = subField.Capacity,
                Description = subField.Description
            };
        }

        public async Task<SubFieldDto> UpdateSubFieldAsync(ClaimsPrincipal user, int fieldId, int subFieldId, UpdateSubFieldDto updateSubFieldDto)
        {
            var ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                _logger.LogError("User ID not found in claims");
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
            }

            var field = await _unitOfWork.Repository<Field>().GetByIdAsync(fieldId);
            if (field == null || field.OwnerId != int.Parse(ownerId))
            {
                _logger.LogWarning("Field not found or user not authorized: FieldId={FieldId}, OwnerId={OwnerId}", fieldId, ownerId);
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật sân nhỏ của sân này.");
            }

            var subField = await _unitOfWork.Repository<SubField>().GetByIdAsync(subFieldId);
            if (subField == null || subField.FieldId != fieldId)
            {
                _logger.LogWarning("SubField not found: SubFieldId={SubFieldId}, FieldId={FieldId}", subFieldId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy sân nhỏ.");
            }

            subField.SubFieldName = updateSubFieldDto.SubFieldName;
            subField.FieldType = updateSubFieldDto.FieldType;
            subField.Status = updateSubFieldDto.Status ?? subField.Status;
            subField.Capacity = updateSubFieldDto.Capacity;
            subField.Description = updateSubFieldDto.Description;

            _unitOfWork.Repository<SubField>().Update(subField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("SubField updated for field: {FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
            return new SubFieldDto
            {
                SubFieldId = subField.SubFieldId,
                SubFieldName = subField.SubFieldName,
                FieldType = subField.FieldType,
                Status = subField.Status,
                Capacity = subField.Capacity,
                Description = subField.Description
            };
        }

        public async Task DeleteSubFieldAsync(ClaimsPrincipal user, int fieldId, int subFieldId)
        {
            var ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                _logger.LogError("User ID not found in claims");
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
            }

            var field = await _unitOfWork.Repository<Field>().GetByIdAsync(fieldId);
            if (field == null || field.OwnerId != int.Parse(ownerId))
            {
                _logger.LogWarning("Field not found or user not authorized: FieldId={FieldId}, OwnerId={OwnerId}", fieldId, ownerId);
                throw new UnauthorizedAccessException("Bạn không có quyền xóa sân nhỏ của sân này.");
            }

            var subField = await _unitOfWork.Repository<SubField>().GetByIdAsync(subFieldId);
            if (subField == null || subField.FieldId != fieldId)
            {
                _logger.LogWarning("SubField not found: SubFieldId={SubFieldId}, FieldId={FieldId}", subFieldId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy sân nhỏ.");
            }

            _unitOfWork.Repository<SubField>().Remove(subField);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("SubField deleted from field: {FieldId}, SubFieldId={SubFieldId}", fieldId, subFieldId);
        }
    }
}