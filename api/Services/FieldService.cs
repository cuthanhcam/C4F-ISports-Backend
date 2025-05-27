using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Field.SubFieldDtos;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using api.Dtos.Field;
using api.Dtos.Field.AddressValidationDtos;
using api.Dtos.Field.FieldImageDtos;
using api.Dtos.Field.FieldServiceDtos;
using api.Dtos.Field.FieldAmenityDtos;
using api.Dtos.Field.FieldDescriptionDtos;
using api.Dtos.Field.FieldPricingDtos;
using api.Dtos.Field.FieldAvailabilityDtos;

namespace api.Services
{
    public class FieldService : IFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IGeocodingService _geocodingService;
        private readonly ILogger<FieldService> _logger;

        public FieldService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IGeocodingService geocodingService, ILogger<FieldService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
            _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FieldDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto)
        {
            var ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                _logger.LogError("User ID not found in claims");
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
            }

            var addressValidation = await _geocodingService.ValidateAddressAsync(new ValidateAddressDto
            {
                FieldName = createFieldDto.FieldName,
                Address = createFieldDto.Address,
                City = createFieldDto.City,
                District = createFieldDto.District
            });

            if (!addressValidation.IsValid)
            {
                _logger.LogWarning("Invalid address for field: {FieldName}", createFieldDto.FieldName);
                throw new ArgumentException("Địa chỉ không hợp lệ.");
            }

            var openHoursParts = createFieldDto.OpenHours.Split('-');
            var openTime = TimeSpan.Parse(openHoursParts[0]);
            var closeTime = TimeSpan.Parse(openHoursParts[1]);

            var field = new Field
            {
                FieldName = createFieldDto.FieldName,
                Phone = createFieldDto.Phone,
                Address = addressValidation.FormattedAddress,
                City = createFieldDto.City,
                District = createFieldDto.District,
                OpenHours = createFieldDto.OpenHours,
                OpenTime = openTime,
                CloseTime = closeTime,
                Status = createFieldDto.Status,
                SportId = createFieldDto.SportId,
                OwnerId = int.Parse(ownerId),
                // Latitude = addressValidation.Latitude,
                // Longitude = addressValidation.Longitude,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Field>().AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Field created successfully: {FieldId}", field.FieldId);
            return MapToFieldDto(field);
        }

        public async Task<FieldDto> UpdateFieldAsync(ClaimsPrincipal user, int fieldId, UpdateFieldDto updateFieldDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật sân này.");
            }

            var addressValidation = await _geocodingService.ValidateAddressAsync(new ValidateAddressDto
            {
                FieldName = updateFieldDto.FieldName,
                Address = updateFieldDto.Address,
                City = updateFieldDto.City,
                District = updateFieldDto.District
            });

            if (!addressValidation.IsValid)
            {
                _logger.LogWarning("Invalid address for field update: {FieldName}", updateFieldDto.FieldName);
                throw new ArgumentException("Địa chỉ không hợp lệ.");
            }

            var openHoursParts = updateFieldDto.OpenHours.Split('-');
            var openTime = TimeSpan.Parse(openHoursParts[0]);
            var closeTime = TimeSpan.Parse(openHoursParts[1]);

            field.FieldName = updateFieldDto.FieldName;
            field.Phone = updateFieldDto.Phone;
            field.Address = addressValidation.FormattedAddress;
            field.City = updateFieldDto.City ?? field.City;
            field.District = updateFieldDto.District ?? field.District;
            field.OpenHours = updateFieldDto.OpenHours;
            field.OpenTime = openTime;
            field.CloseTime = closeTime;
            field.Status = updateFieldDto.Status;
            field.SportId = updateFieldDto.SportId;
            // field.Latitude = addressValidation.Latitude;
            // field.Longitude = addressValidation.Longitude;
            field.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Field>().Update(field);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Field updated successfully: {FieldId}", field.FieldId);
            return MapToFieldDto(field);
        }

        public async Task DeleteFieldAsync(ClaimsPrincipal user, int fieldId)
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
                throw new UnauthorizedAccessException("Bạn không có quyền xóa sân này.");
            }

            var images = await _unitOfWork.Repository<FieldImage>()
                .FindAsync(fi => fi.FieldId == fieldId);
            foreach (var image in images)
            {
                if (!string.IsNullOrEmpty(image.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(image.PublicId);
                }
            }

            _unitOfWork.Repository<Field>().Remove(field);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Field deleted successfully: {FieldId}", fieldId);
        }

        public async Task<FieldDto> GetFieldByIdAsync(int fieldId)
        {
            var field = await _unitOfWork.Context.Fields
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.FieldPricings)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldDescriptions)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId);

            if (field == null)
            {
                _logger.LogWarning("Field not found: {FieldId}", fieldId);
                throw new KeyNotFoundException("Không tìm thấy sân.");
            }

            _logger.LogInformation("Field retrieved: {FieldId}", fieldId);
            return MapToFieldDto(field);
        }

        public async Task<IEnumerable<FieldDto>> GetAllFieldsAsync()
        {
            var fields = await _unitOfWork.Context.Fields
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.FieldPricings)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldDescriptions)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} fields", fields.Count);
            return fields.Select(MapToFieldDto);
        }

        public async Task<FieldImageDto> AddFieldImageAsync(ClaimsPrincipal user, int fieldId, IFormFile image)
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
                throw new UnauthorizedAccessException("Bạn không có quyền thêm ảnh cho sân này.");
            }

            var imageUrl = await _cloudinaryService.UploadImageAsync(image);
            var publicId = imageUrl.Split('/').Last().Split('.')[0];

            var fieldImage = new FieldImage
            {
                FieldId = fieldId,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsPrimary = !field.FieldImages.Any(),
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FieldImage>().AddAsync(fieldImage);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Image added to field: {FieldId}, ImageId={ImageId}", fieldId, fieldImage.FieldImageId);
            return new FieldImageDto
            {
                FieldImageId = fieldImage.FieldImageId,
                ImageUrl = fieldImage.ImageUrl,
                IsPrimary = fieldImage.IsPrimary,
                UploadedAt = fieldImage.UploadedAt
            };
        }

        public async Task DeleteFieldImageAsync(ClaimsPrincipal user, int fieldId, int imageId)
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
                throw new UnauthorizedAccessException("Bạn không có quyền xóa ảnh của sân này.");
            }

            var image = await _unitOfWork.Repository<FieldImage>().GetByIdAsync(imageId);
            if (image == null || image.FieldId != fieldId)
            {
                _logger.LogWarning("Image not found: ImageId={ImageId}, FieldId={FieldId}", imageId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy ảnh.");
            }

            if (!string.IsNullOrEmpty(image.PublicId))
            {
                await _cloudinaryService.DeleteImageAsync(image.PublicId);
            }

            _unitOfWork.Repository<FieldImage>().Remove(image);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Image deleted from field: {FieldId}, ImageId={ImageId}", fieldId, imageId);
        }

        public async Task<FieldServiceDto> CreateFieldServiceAsync(ClaimsPrincipal user, int fieldId, CreateFieldServiceDto createFieldServiceDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền thêm dịch vụ cho sân này.");
            }

            var fieldService = new Models.FieldService
            {
                FieldId = fieldId,
                ServiceName = createFieldServiceDto.ServiceName,
                Price = createFieldServiceDto.Price,
                Description = createFieldServiceDto.Description,
                IsActive = createFieldServiceDto.IsActive
            };

            await _unitOfWork.Repository<Models.FieldService>().AddAsync(fieldService);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Service added to field: {FieldId}, ServiceId={ServiceId}", fieldId, fieldService.FieldServiceId);
            return new FieldServiceDto
            {
                FieldServiceId = fieldService.FieldServiceId,
                ServiceName = fieldService.ServiceName,
                Price = fieldService.Price,
                Description = fieldService.Description,
                IsActive = fieldService.IsActive
            };
        }

        public async Task<FieldServiceDto> UpdateFieldServiceAsync(ClaimsPrincipal user, int fieldId, int serviceId, UpdateFieldServiceDto updateFieldServiceDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật dịch vụ của sân này.");
            }

            var fieldService = await _unitOfWork.Repository<Models.FieldService>().GetByIdAsync(serviceId);
            if (fieldService == null || fieldService.FieldId != fieldId)
            {
                _logger.LogWarning("Service not found: ServiceId={ServiceId}, FieldId={FieldId}", serviceId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy dịch vụ.");
            }

            fieldService.ServiceName = updateFieldServiceDto.ServiceName;
            fieldService.Price = updateFieldServiceDto.Price;
            fieldService.Description = updateFieldServiceDto.Description;
            fieldService.IsActive = updateFieldServiceDto.IsActive;

            _unitOfWork.Repository<Models.FieldService>().Update(fieldService);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Service updated for field: {FieldId}, ServiceId={ServiceId}", fieldId, serviceId);
            return new FieldServiceDto
            {
                FieldServiceId = fieldService.FieldServiceId,
                ServiceName = fieldService.ServiceName,
                Price = fieldService.Price,
                Description = fieldService.Description,
                IsActive = fieldService.IsActive
            };
        }

        public async Task<FieldAmenityDto> CreateFieldAmenityAsync(ClaimsPrincipal user, int fieldId, CreateFieldAmenityDto createFieldAmenityDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền thêm tiện ích cho sân này.");
            }

            var fieldAmenity = new FieldAmenity
            {
                FieldId = fieldId,
                AmenityName = createFieldAmenityDto.AmenityName,
                Description = createFieldAmenityDto.Description,
                IconUrl = createFieldAmenityDto.IconUrl
            };

            await _unitOfWork.Repository<FieldAmenity>().AddAsync(fieldAmenity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Amenity added to field: {FieldId}, AmenityId={AmenityId}", fieldId, fieldAmenity.FieldAmenityId);
            return new FieldAmenityDto
            {
                FieldAmenityId = fieldAmenity.FieldAmenityId,
                AmenityName = fieldAmenity.AmenityName,
                Description = fieldAmenity.Description,
                IconUrl = fieldAmenity.IconUrl
            };
        }

        public async Task<FieldAmenityDto> UpdateFieldAmenityAsync(ClaimsPrincipal user, int fieldId, int amenityId, UpdateFieldAmenityDto updateFieldAmenityDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật tiện ích của sân này.");
            }

            var fieldAmenity = await _unitOfWork.Repository<FieldAmenity>().GetByIdAsync(amenityId);
            if (fieldAmenity == null || fieldAmenity.FieldId != fieldId)
            {
                _logger.LogWarning("Amenity not found: AmenityId={AmenityId}, FieldId={FieldId}", amenityId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy tiện ích.");
            }

            fieldAmenity.AmenityName = updateFieldAmenityDto.AmenityName;
            fieldAmenity.Description = updateFieldAmenityDto.Description;
            fieldAmenity.IconUrl = updateFieldAmenityDto.IconUrl;

            _unitOfWork.Repository<FieldAmenity>().Update(fieldAmenity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Amenity updated for field: {FieldId}, AmenityId={AmenityId}", fieldId, amenityId);
            return new FieldAmenityDto
            {
                FieldAmenityId = fieldAmenity.FieldAmenityId,
                AmenityName = fieldAmenity.AmenityName,
                Description = fieldAmenity.Description,
                IconUrl = fieldAmenity.IconUrl
            };
        }

        public async Task<FieldDescriptionDto> CreateFieldDescriptionAsync(ClaimsPrincipal user, int fieldId, CreateFieldDescriptionDto createFieldDescriptionDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền thêm mô tả cho sân này.");
            }

            var fieldDescription = new FieldDescription
            {
                FieldId = fieldId,
                Description = createFieldDescriptionDto.Description
            };

            await _unitOfWork.Repository<FieldDescription>().AddAsync(fieldDescription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Description added to field: {FieldId}, DescriptionId={DescriptionId}", fieldId, fieldDescription.FieldDescriptionId);
            return new FieldDescriptionDto
            {
                FieldDescriptionId = fieldDescription.FieldDescriptionId,
                Description = fieldDescription.Description
            };
        }

        public async Task<FieldDescriptionDto> UpdateFieldDescriptionAsync(ClaimsPrincipal user, int fieldId, int descriptionId, UpdateFieldDescriptionDto updateFieldDescriptionDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật mô tả của sân này.");
            }

            var fieldDescription = await _unitOfWork.Repository<FieldDescription>().GetByIdAsync(descriptionId);
            if (fieldDescription == null || fieldDescription.FieldId != fieldId)
            {
                _logger.LogWarning("Description not found: DescriptionId={DescriptionId}, FieldId={FieldId}", descriptionId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy mô tả.");
            }

            fieldDescription.Description = updateFieldDescriptionDto.Description;

            _unitOfWork.Repository<FieldDescription>().Update(fieldDescription);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Description updated for field: {FieldId}, DescriptionId={DescriptionId}", fieldId, descriptionId);
            return new FieldDescriptionDto
            {
                FieldDescriptionId = fieldDescription.FieldDescriptionId,
                Description = fieldDescription.Description
            };
        }

        public async Task<FieldPricingDto> CreateFieldPricingAsync(ClaimsPrincipal user, int fieldId, CreateFieldPricingDto createFieldPricingDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền thêm giá cho sân này.");
            }

            var subField = await _unitOfWork.Repository<SubField>().GetByIdAsync(createFieldPricingDto.SubFieldId);
            if (subField == null || subField.FieldId != fieldId)
            {
                _logger.LogWarning("SubField not found: SubFieldId={SubFieldId}, FieldId={FieldId}", createFieldPricingDto.SubFieldId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy sân nhỏ.");
            }

            var fieldPricing = new FieldPricing
            {
                SubFieldId = createFieldPricingDto.SubFieldId,
                StartTime = TimeSpan.Parse(createFieldPricingDto.StartTime),
                EndTime = TimeSpan.Parse(createFieldPricingDto.EndTime),
                DayOfWeek = createFieldPricingDto.DayOfWeek,
                Price = createFieldPricingDto.Price,
                IsActive = createFieldPricingDto.IsActive
            };

            await _unitOfWork.Repository<FieldPricing>().AddAsync(fieldPricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pricing added to field: {FieldId}, PricingId={PricingId}", fieldId, fieldPricing.FieldPricingId);
            return new FieldPricingDto
            {
                FieldPricingId = fieldPricing.FieldPricingId,
                SubFieldId = fieldPricing.SubFieldId,
                StartTime = fieldPricing.StartTime.ToString(@"hh\:mm"),
                EndTime = fieldPricing.EndTime.ToString(@"hh\:mm"),
                DayOfWeek = fieldPricing.DayOfWeek,
                Price = fieldPricing.Price,
                IsActive = fieldPricing.IsActive
            };
        }

        public async Task<FieldPricingDto> UpdateFieldPricingAsync(ClaimsPrincipal user, int fieldId, int pricingId, UpdateFieldPricingDto updateFieldPricingDto)
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
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật giá của sân này.");
            }

            var fieldPricing = await _unitOfWork.Repository<FieldPricing>().GetByIdAsync(pricingId);
            if (fieldPricing == null || fieldPricing.SubField.FieldId != fieldId)
            {
                _logger.LogWarning("Pricing not found: PricingId={PricingId}, FieldId={FieldId}", pricingId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy giá.");
            }

            var subField = await _unitOfWork.Repository<SubField>().GetByIdAsync(updateFieldPricingDto.SubFieldId);
            if (subField == null || subField.FieldId != fieldId)
            {
                _logger.LogWarning("SubField not found: SubFieldId={SubFieldId}, FieldId={FieldId}", updateFieldPricingDto.SubFieldId, fieldId);
                throw new KeyNotFoundException("Không tìm thấy sân nhỏ.");
            }

            fieldPricing.SubFieldId = updateFieldPricingDto.SubFieldId;
            fieldPricing.StartTime = TimeSpan.Parse(updateFieldPricingDto.StartTime);
            fieldPricing.EndTime = TimeSpan.Parse(updateFieldPricingDto.EndTime);
            fieldPricing.DayOfWeek = updateFieldPricingDto.DayOfWeek;
            fieldPricing.Price = updateFieldPricingDto.Price;
            fieldPricing.IsActive = updateFieldPricingDto.IsActive;

            _unitOfWork.Repository<FieldPricing>().Update(fieldPricing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pricing updated for field: {FieldId}, PricingId={PricingId}", fieldId, pricingId);
            return new FieldPricingDto
            {
                FieldPricingId = fieldPricing.FieldPricingId,
                SubFieldId = fieldPricing.SubFieldId,
                StartTime = fieldPricing.StartTime.ToString(@"hh\:mm"),
                EndTime = fieldPricing.EndTime.ToString(@"hh\:mm"),
                DayOfWeek = fieldPricing.DayOfWeek,
                Price = fieldPricing.Price,
                IsActive = fieldPricing.IsActive
            };
        }

        public async Task<IEnumerable<FieldAvailabilityDto>> GetFieldAvailabilityAsync(int? fieldId, DateTime? date)
        {
            var query = _unitOfWork.Context.Fields.AsQueryable();
            if (fieldId.HasValue)
            {
                query = query.Where(f => f.FieldId == fieldId.Value);
            }

            var fields = await query
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.FieldPricings)
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.Bookings)
                .ToListAsync();

            var availability = new List<FieldAvailabilityDto>();
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            foreach (var field in fields)
            {
                foreach (var subField in field.SubFields)
                {
                    var pricings = subField.FieldPricings
                        .Where(p => p.IsActive && p.DayOfWeek == (int)targetDate.DayOfWeek)
                        .ToList();

                    foreach (var pricing in pricings)
                    {
                        var isBooked = subField.Bookings.Any(b =>
                            b.BookingDate.Date == targetDate &&
                            b.StartTime < pricing.EndTime &&
                            b.EndTime > pricing.StartTime);

                        if (!isBooked)
                        {
                            var promotion = await _unitOfWork.Context.Promotions
                                .Where(p => p.IsActive &&
                                            p.StartDate <= targetDate && p.EndDate >= targetDate)
                                .Select(p => new PromotionDto
                                {
                                    PromotionId = p.PromotionId,
                                    PromotionCode = p.Code,
                                    // DiscountValue = p.DiscountValue,
                                    DiscountType = p.DiscountType
                                })
                                .FirstOrDefaultAsync();

                            availability.Add(new FieldAvailabilityDto
                            {
                                FieldId = field.FieldId,
                                FieldName = field.FieldName,
                                SubFieldId = subField.SubFieldId,
                                SubFieldName = subField.SubFieldName,
                                Date = targetDate,
                                StartTime = pricing.StartTime.ToString(@"hh\:mm"),
                                EndTime = pricing.EndTime.ToString(@"hh\:mm"),
                                Price = pricing.Price,
                                Promotion = promotion
                            });
                        }
                    }
                }
            }

            _logger.LogInformation("Retrieved {Count} available slots for date: {Date}", availability.Count, targetDate);
            return availability;
        }

        public async Task<AddressValidationResultDto> ValidateAddressAsync(ValidateAddressDto addressDto)
        {
            var result = await _geocodingService.ValidateAddressAsync(addressDto);
            _logger.LogInformation("Address validation result: IsValid={IsValid}, FormattedAddress={FormattedAddress}", result.IsValid, result.FormattedAddress);
            return result;
        }

        private FieldDto MapToFieldDto(Field field)
        {
            return new FieldDto
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                Phone = field.Phone,
                Address = field.Address,
                City = field.City,
                District = field.District,
                OpenHours = field.OpenHours,
                OpenTime = field.OpenTime?.ToString(@"hh\:mm"),
                CloseTime = field.CloseTime?.ToString(@"hh\:mm"),
                Status = field.Status,
                // Latitude = field.Latitude,
                // Longitude = field.Longitude,
                AverageRating = field.AverageRating,
                SportId = field.SportId,
                SubFields = field.SubFields.Select(sf => new SubFieldDto
                {
                    SubFieldId = sf.SubFieldId,
                    SubFieldName = sf.SubFieldName,
                    FieldType = sf.FieldType,
                    Status = sf.Status,
                    Capacity = sf.Capacity,
                    Description = sf.Description
                }).ToList(),
                FieldImages = field.FieldImages.Select(i => new FieldImageDto
                {
                    FieldImageId = i.FieldImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    UploadedAt = i.UploadedAt
                }).ToList(),
                FieldServices = field.FieldServices.Select(s => new FieldServiceDto
                {
                    FieldServiceId = s.FieldServiceId,
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Description = s.Description,
                    IsActive = s.IsActive
                }).ToList(),
                FieldAmenities = field.FieldAmenities.Select(a => new FieldAmenityDto
                {
                    FieldAmenityId = a.FieldAmenityId,
                    AmenityName = a.AmenityName,
                    Description = a.Description,
                    IconUrl = a.IconUrl
                }).ToList(),
                FieldDescriptions = field.FieldDescriptions.Select(d => new FieldDescriptionDto
                {
                    FieldDescriptionId = d.FieldDescriptionId,
                    Description = d.Description
                }).ToList(),
                FieldPricings = field.SubFields.SelectMany(sf => sf.FieldPricings).Select(p => new FieldPricingDto
                {
                    FieldPricingId = p.FieldPricingId,
                    SubFieldId = p.SubFieldId,
                    StartTime = p.StartTime.ToString(@"hh\:mm"),
                    EndTime = p.EndTime.ToString(@"hh\:mm"),
                    DayOfWeek = p.DayOfWeek,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList()
            };
        }
    }
}