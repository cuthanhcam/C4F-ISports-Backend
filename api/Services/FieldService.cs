using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Field;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace api.Services
{
    public class FieldService : IFieldService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeocodingService _geocodingService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<FieldService> _logger;

        public FieldService(
            ApplicationDbContext context,
            IGeocodingService geocodingService,
            ICloudinaryService cloudinaryService,
            IDistributedCache cache,
            ILogger<FieldService> logger)
        {
            _context = context;
            _geocodingService = geocodingService;
            _cloudinaryService = cloudinaryService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<(List<FieldResponseDto> Data, int Total, int Page, int PageSize)> GetFilteredFieldsAsync(FieldFilterDto filterDto)
        {
            var cacheKey = $"fields_{JsonSerializer.Serialize(filterDto)}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                return JsonSerializer.Deserialize<(List<FieldResponseDto>, int, int, int)>(cachedData);
            }

            var query = _context.Fields
                .Where(f => f.Status != "Deleted" && f.DeletedAt == null)
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.PricingRules)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterDto.City))
                query = query.Where(f => f.City == filterDto.City);
            if (!string.IsNullOrEmpty(filterDto.District))
                query = query.Where(f => f.District == filterDto.District);
            if (filterDto.SportId.HasValue)
                query = query.Where(f => f.SportId == filterDto.SportId);
            if (!string.IsNullOrEmpty(filterDto.Search))
                query = query.Where(f => f.FieldName.Contains(filterDto.Search) || f.Address.Contains(filterDto.Search));
            if (filterDto.MinPrice.HasValue || filterDto.MaxPrice.HasValue)
            {
                query = query.Where(f => f.SubFields.Any(sf => sf.PricingRules.Any(pr => pr.TimeSlots.Any(ts =>
                    (!filterDto.MinPrice.HasValue || ts.PricePerSlot >= filterDto.MinPrice) &&
                    (!filterDto.MaxPrice.HasValue || ts.PricePerSlot <= filterDto.MaxPrice)))));
            }

            if (filterDto.Latitude.HasValue && filterDto.Longitude.HasValue)
            {
                // Haversine formula for distance
                query = query.OrderBy(f => 6371 * Math.Acos(
                    Math.Cos(filterDto.Latitude.Value * Math.PI / 180) *
                    Math.Cos(f.Latitude * Math.PI / 180) *
                    Math.Cos(f.Longitude * Math.PI / 180 - filterDto.Longitude.Value * Math.PI / 180) +
                    Math.Sin(filterDto.Latitude.Value * Math.PI / 180) *
                    Math.Sin(f.Latitude * Math.PI / 180)));
            }
            else
            {
                if (filterDto.SortBy == "averageRating")
                    query = filterDto.SortOrder == "desc" ? query.OrderByDescending(f => f.AverageRating) : query.OrderBy(f => f.AverageRating);
                else if (filterDto.SortBy == "price")
                    query = filterDto.SortOrder == "desc"
                        ? query.OrderByDescending(f => f.SubFields.Min(sf => sf.DefaultPricePerSlot))
                        : query.OrderBy(f => f.SubFields.Min(sf => sf.DefaultPricePerSlot));
                else
                    query = filterDto.SortOrder == "desc" ? query.OrderByDescending(f => f.FieldId) : query.OrderBy(f => f.FieldId);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((filterDto.Page - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .Select(f => new FieldResponseDto
                {
                    FieldId = f.FieldId,
                    FieldName = f.FieldName,
                    Description = f.Description,
                    Address = f.Address,
                    City = f.City,
                    District = f.District,
                    Latitude = f.Latitude,
                    Longitude = f.Longitude,
                    OpenTime = f.OpenTime.ToString(@"HH\:mm"),
                    CloseTime = f.CloseTime.ToString(@"HH\:mm"),
                    AverageRating = f.AverageRating,
                    SportId = f.SportId,
                    Distance = filterDto.Latitude.HasValue && filterDto.Longitude.HasValue
                        ? 6371 * Math.Acos(
                            Math.Cos(filterDto.Latitude.Value * Math.PI / 180) *
                            Math.Cos(f.Latitude * Math.PI / 180) *
                            Math.Cos(f.Longitude * Math.PI / 180 - filterDto.Longitude.Value * Math.PI / 180) +
                            Math.Sin(filterDto.Latitude.Value * Math.PI / 180) *
                            Math.Sin(f.Latitude * Math.PI / 180))
                        : null,
                    MinPricePerSlot = f.SubFields.Min(sf => sf.PricingRules.Any() ? sf.PricingRules.Min(pr => pr.TimeSlots.Min(ts => ts.PricePerSlot)) : sf.DefaultPricePerSlot),
                    MaxPricePerSlot = f.SubFields.Max(sf => sf.PricingRules.Any() ? sf.PricingRules.Max(pr => pr.TimeSlots.Max(ts => ts.PricePerSlot)) : sf.DefaultPricePerSlot),
                    SubFields = f.SubFields.Select(sf => new SubFieldResponseDto
                    {
                        SubFieldId = sf.SubFieldId,
                        SubFieldName = sf.SubFieldName,
                        FieldType = sf.FieldType,
                        Description = sf.Description,
                        Status = sf.Status,
                        Capacity = sf.Capacity,
                        OpenTime = sf.OpenTime.ToString(@"HH\:mm"),
                        CloseTime = sf.CloseTime.ToString(@"HH\:mm"),
                        DefaultPricePerSlot = sf.DefaultPricePerSlot,
                        PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                        {
                            PricingRuleId = pr.PricingRuleId,
                            AppliesToDays = pr.AppliesToDays,
                            TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                            {
                                StartTime = ts.StartTime.ToString(@"HH\:mm"),
                                EndTime = ts.EndTime.ToString(@"HH\:mm"),
                                PricePerSlot = ts.PricePerSlot
                            }).ToList()
                        }).ToList(),
                        Parent7aSideId = sf.Parent7aSideId,
                        Child5aSideIds = sf.Child5aSideIds
                    }).ToList(),
                    Services = f.FieldServices.Select(fs => new FieldServiceResponseDto
                    {
                        FieldServiceId = fs.FieldServiceId,
                        ServiceName = fs.ServiceName,
                        Price = fs.Price,
                        Description = fs.Description,
                        IsActive = fs.IsActive
                    }).ToList(),
                    Amenities = f.FieldAmenities.Select(fa => new FieldAmenityResponseDto
                    {
                        FieldAmenityId = fa.FieldAmenityId,
                        AmenityName = fa.AmenityName,
                        Description = fa.Description,
                        IconUrl = fa.IconUrl
                    }).ToList(),
                    Images = f.FieldImages.Select(fi => new FieldImageResponseDto
                    {
                        FieldImageId = fi.FieldImageId,
                        ImageUrl = fi.ImageUrl,
                        PublicId = fi.PublicId,
                        Thumbnail = fi.Thumbnail,
                        IsPrimary = fi.IsPrimary,
                        UploadedAt = fi.UploadedAt
                    }).ToList()
                }).ToListAsync();

            var result = (data, total, filterDto.Page, filterDto.PageSize);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return result;
        }

        public async Task<FieldResponseDto> GetFieldByIdAsync(int fieldId, string? include)
        {
            var cacheKey = $"field_{fieldId}_{include ?? "none"}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Lấy dữ liệu sân ID {FieldId} từ cache, include: {Include}", fieldId, include);
                return JsonSerializer.Deserialize<FieldResponseDto>(cachedData);
            }

            _logger.LogInformation("Truy vấn sân ID {FieldId} từ database, include: {Include}", fieldId, include);
            var query = _context.Fields
                .Where(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null)
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.PricingRules)
                .AsSplitQuery(); // Tách truy vấn để tối ưu hiệu suất

            if (!string.IsNullOrEmpty(include))
            {
                var includes = include.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries);
                _logger.LogDebug("Các include được yêu cầu: {Includes}", string.Join(", ", includes));

                if (includes.Contains("services"))
                    query = query.Include(f => f.FieldServices);
                if (includes.Contains("amenities"))
                    query = query.Include(f => f.FieldAmenities);
                if (includes.Contains("images"))
                    query = query.Include(f => f.FieldImages);
            }

            var field = await query
                .Select(f => new FieldResponseDto
                {
                    FieldId = f.FieldId,
                    FieldName = f.FieldName,
                    Description = f.Description,
                    Address = f.Address,
                    City = f.City,
                    District = f.District,
                    Latitude = f.Latitude,
                    Longitude = f.Longitude,
                    OpenTime = f.OpenTime.ToString(@"HH\:mm"),
                    CloseTime = f.CloseTime.ToString(@"HH\:mm"),
                    AverageRating = f.AverageRating,
                    SportId = f.SportId,
                    MinPricePerSlot = f.SubFields.Any()
                        ? f.SubFields.Min(sf => sf.PricingRules.Any() && sf.PricingRules.Any(pr => pr.TimeSlots.Any())
                            ? sf.PricingRules.Min(pr => pr.TimeSlots.Any() ? pr.TimeSlots.Min(ts => ts.PricePerSlot) : sf.DefaultPricePerSlot)
                            : sf.DefaultPricePerSlot)
                        : 0,
                    MaxPricePerSlot = f.SubFields.Any()
                        ? f.SubFields.Max(sf => sf.PricingRules.Any() && sf.PricingRules.Any(pr => pr.TimeSlots.Any())
                            ? sf.PricingRules.Max(pr => pr.TimeSlots.Any() ? pr.TimeSlots.Max(ts => ts.PricePerSlot) : sf.DefaultPricePerSlot)
                            : sf.DefaultPricePerSlot)
                        : 0,
                    SubFields = f.SubFields.Select(sf => new SubFieldResponseDto
                    {
                        SubFieldId = sf.SubFieldId,
                        SubFieldName = sf.SubFieldName,
                        FieldType = sf.FieldType,
                        Description = sf.Description,
                        Status = sf.Status,
                        Capacity = sf.Capacity,
                        OpenTime = sf.OpenTime.ToString(@"HH\:mm"),
                        CloseTime = sf.CloseTime.ToString(@"HH\:mm"),
                        DefaultPricePerSlot = sf.DefaultPricePerSlot,
                        PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                        {
                            PricingRuleId = pr.PricingRuleId,
                            AppliesToDays = pr.AppliesToDays,
                            TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                            {
                                StartTime = ts.StartTime.ToString(@"HH\:mm"),
                                EndTime = ts.EndTime.ToString(@"HH\:mm"),
                                PricePerSlot = ts.PricePerSlot
                            }).ToList()
                        }).ToList(),
                        Parent7aSideId = sf.Parent7aSideId,
                        Child5aSideIds = sf.Child5aSideIds
                    }).ToList(),
                    Services = f.FieldServices.Select(fs => new FieldServiceResponseDto
                    {
                        FieldServiceId = fs.FieldServiceId,
                        ServiceName = fs.ServiceName,
                        Price = fs.Price,
                        Description = fs.Description,
                        IsActive = fs.IsActive
                    }).ToList(),
                    Amenities = f.FieldAmenities.Select(fa => new FieldAmenityResponseDto
                    {
                        FieldAmenityId = fa.FieldAmenityId,
                        AmenityName = fa.AmenityName,
                        Description = fa.Description,
                        IconUrl = fa.IconUrl
                    }).ToList(),
                    Images = f.FieldImages.Select(fi => new FieldImageResponseDto
                    {
                        FieldImageId = fi.FieldImageId,
                        ImageUrl = fi.ImageUrl,
                        PublicId = fi.PublicId,
                        Thumbnail = fi.Thumbnail,
                        IsPrimary = fi.IsPrimary,
                        UploadedAt = fi.UploadedAt
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (field == null)
            {
                _logger.LogWarning("Sân ID {FieldId} không tồn tại hoặc đã bị xóa.", fieldId);
                throw new ArgumentException("Field not found");
            }

            _logger.LogInformation("Lưu dữ liệu sân ID {FieldId} vào cache.", fieldId);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(field), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return field;
        }

        public async Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto validateAddressDto)
        {
            var cacheKey = $"address_{validateAddressDto.Address}_{validateAddressDto.City}_{validateAddressDto.District}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                return JsonSerializer.Deserialize<ValidateAddressResponseDto>(cachedData);
            }

            var geocodingResult = await _geocodingService.ValidateAddressAsync(validateAddressDto);
            if (!geocodingResult.IsValid)
                throw new ArgumentException("Invalid address");

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(geocodingResult), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            return geocodingResult;
        }

        public async Task<FieldResponseDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto)
        {
            var ownerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.OwnerId == ownerId);
            if (owner == null)
                throw new UnauthorizedAccessException("User is not an owner");

            var sport = await _context.Sports.FirstOrDefaultAsync(s => s.SportId == createFieldDto.SportId);
            if (sport == null)
                throw new ArgumentException("Sport not found");

            var addressValidation = await ValidateAddressAsync(new ValidateAddressDto
            {
                FieldName = createFieldDto.FieldName,
                Address = createFieldDto.Address,
                City = createFieldDto.City,
                District = createFieldDto.District
            });

            if (createFieldDto.SubFields == null || createFieldDto.SubFields.Count == 0)
                throw new ArgumentException("At least one subfield is required");

            if (createFieldDto.SubFields.Count > 10)
                throw new ArgumentException("Maximum 10 subfields allowed");

            var field = new Field
            {
                FieldName = createFieldDto.FieldName,
                Description = createFieldDto.Description,
                Address = createFieldDto.Address,
                City = createFieldDto.City,
                District = createFieldDto.District,
                OpenTime = TimeSpan.Parse(createFieldDto.OpenTime),
                CloseTime = TimeSpan.Parse(createFieldDto.CloseTime),
                SportId = createFieldDto.SportId,
                OwnerId = ownerId,
                Latitude = addressValidation.Latitude,
                Longitude = addressValidation.Longitude,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            field.SubFields = createFieldDto.SubFields.Select(sf => new SubField
            {
                SubFieldName = sf.SubFieldName,
                FieldType = sf.FieldType,
                Description = sf.Description,
                Capacity = sf.Capacity,
                OpenTime = TimeSpan.Parse(sf.OpenTime),
                CloseTime = TimeSpan.Parse(sf.CloseTime),
                DefaultPricePerSlot = sf.DefaultPricePerSlot,
                Status = "Active",
                Parent7aSideId = sf.Parent7aSideId,
                Child5aSideIds = sf.Child5aSideIds,
                PricingRules = sf.PricingRules?.Select(pr => new PricingRule
                {
                    AppliesToDays = pr.AppliesToDays,
                    TimeSlots = pr.TimeSlots.Select(ts => new TimeSlot
                    {
                        StartTime = TimeSpan.Parse(ts.StartTime),
                        EndTime = TimeSpan.Parse(ts.EndTime),
                        PricePerSlot = ts.PricePerSlot
                    }).ToList()
                }).ToList() ?? new List<PricingRule>()
            }).ToList();

            if (createFieldDto.Services?.Count > 50)
                throw new ArgumentException("Maximum 50 services allowed");
            field.FieldServices = createFieldDto.Services?.Select(fs => new api.Models.FieldService
            {
                ServiceName = fs.ServiceName,
                Price = fs.Price,
                Description = fs.Description,
                IsActive = true
            }).ToList() ?? new List<api.Models.FieldService>();

            if (createFieldDto.Amenities?.Count > 50)
                throw new ArgumentException("Maximum 50 amenities allowed");
            field.FieldAmenities = createFieldDto.Amenities?.Select(fa => new FieldAmenity
            {
                AmenityName = fa.AmenityName,
                Description = fa.Description,
                IconUrl = fa.IconUrl
            }).ToList() ?? new List<FieldAmenity>();

            if (createFieldDto.Images?.Count > 50)
                throw new ArgumentException("Maximum 50 images allowed");
            field.FieldImages = createFieldDto.Images?.Select(fi => new FieldImage
            {
                ImageUrl = fi.ImageUrl,
                PublicId = fi.PublicId,
                Thumbnail = fi.Thumbnail,
                IsPrimary = fi.IsPrimary,
                UploadedAt = DateTime.UtcNow
            }).ToList() ?? new List<FieldImage>();

            _context.Fields.Add(field);
            await _context.SaveChangesAsync();

            var cacheKey = $"field_{field.FieldId}_none";
            var response = await GetFieldByIdAsync(field.FieldId, null);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return response;
        }

        public async Task<FieldImageResponseDto> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, IFormFile image, bool isPrimary)
        {
            var ownerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var field = await _context.Fields
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null && f.OwnerId == ownerId);
            if (field == null)
                throw new UnauthorizedAccessException("Field not found or user is not the owner");

            var imageUrl = await _cloudinaryService.UploadImageAsync(image);
            var uploadResult = await _cloudinaryService.UploadImageAsync(image);
            var fieldImage = new FieldImage
            {
                FieldId = fieldId,
                ImageUrl = uploadResult.Url,
                PublicId = uploadResult.PublicId,
                Thumbnail = uploadResult.Url,
                IsPrimary = isPrimary,
                UploadedAt = DateTime.UtcNow
            };

            _context.FieldImages.Add(fieldImage);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"field_{fieldId}_images");
            await _cache.RemoveAsync($"field_{fieldId}_none");

            return new FieldImageResponseDto
            {
                FieldImageId = fieldImage.FieldImageId,
                ImageUrl = fieldImage.ImageUrl,
                PublicId = fieldImage.PublicId,
                Thumbnail = fieldImage.Thumbnail,
                IsPrimary = fieldImage.IsPrimary,
                UploadedAt = fieldImage.UploadedAt
            };
        }

        public async Task<FieldResponseDto> UpdateFieldAsync(ClaimsPrincipal user, int fieldId, UpdateFieldDto updateFieldDto)
        {
            var ownerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var field = await _context.Fields
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.PricingRules)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldImages)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null && f.OwnerId == ownerId);
            if (field == null)
                throw new UnauthorizedAccessException("Field not found or user is not the owner");

            var sport = await _context.Sports.FirstOrDefaultAsync(s => s.SportId == updateFieldDto.SportId);
            if (sport == null)
                throw new ArgumentException("Sport not found");

            var addressValidation = await ValidateAddressAsync(new ValidateAddressDto
            {
                FieldName = updateFieldDto.FieldName,
                Address = updateFieldDto.Address,
                City = updateFieldDto.City,
                District = updateFieldDto.District
            });

            field.FieldName = updateFieldDto.FieldName;
            field.Description = updateFieldDto.Description;
            field.Address = updateFieldDto.Address;
            field.City = updateFieldDto.City;
            field.District = updateFieldDto.District;
            field.OpenTime = TimeSpan.Parse(updateFieldDto.OpenTime);
            field.CloseTime = TimeSpan.Parse(updateFieldDto.CloseTime);
            field.SportId = updateFieldDto.SportId;
            field.Latitude = addressValidation.Latitude;
            field.Longitude = addressValidation.Longitude;
            field.UpdatedAt = DateTime.UtcNow;

            if (field.SubFields?.Any() == true)
                _context.SubFields.RemoveRange(field.SubFields);

            if (updateFieldDto.SubFields == null || updateFieldDto.SubFields.Count == 0)
                throw new ArgumentException("At least one subfield is required");

            if (updateFieldDto.SubFields.Count > 10)
                throw new ArgumentException("Maximum 10 subfields allowed");

            field.SubFields = updateFieldDto.SubFields.Select(sf => new SubField
            {
                SubFieldName = sf.SubFieldName,
                FieldType = sf.FieldType,
                Description = sf.Description,
                Capacity = sf.Capacity,
                OpenTime = TimeSpan.Parse(sf.OpenTime),
                CloseTime = TimeSpan.Parse(sf.CloseTime),
                DefaultPricePerSlot = sf.DefaultPricePerSlot,
                Status = "Active",
                Parent7aSideId = sf.Parent7aSideId,
                Child5aSideIds = sf.Child5aSideIds,
                PricingRules = sf.PricingRules?.Select(pr => new PricingRule
                {
                    AppliesToDays = pr.AppliesToDays,
                    TimeSlots = pr.TimeSlots.Select(ts => new TimeSlot
                    {
                        StartTime = TimeSpan.Parse(ts.StartTime),
                        EndTime = TimeSpan.Parse(ts.EndTime),
                        PricePerSlot = ts.PricePerSlot
                    }).ToList()
                }).ToList() ?? new List<PricingRule>()
            }).ToList();

            if (field.FieldServices?.Any() == true)
                _context.FieldServices.RemoveRange(field.FieldServices);

            if (updateFieldDto.Services?.Count > 50)
                throw new ArgumentException("Maximum 50 services allowed");

            field.FieldServices = updateFieldDto.Services?.Select(fs => new api.Models.FieldService
            {
                ServiceName = fs.ServiceName,
                Price = fs.Price,
                Description = fs.Description,
                IsActive = true
            }).ToList() ?? new List<api.Models.FieldService>();

            if (field.FieldAmenities?.Any() == true)
                _context.FieldAmenities.RemoveRange(field.FieldAmenities);

            if (updateFieldDto.Amenities?.Count > 50)
                throw new ArgumentException("Maximum 50 amenities allowed");

            field.FieldAmenities = updateFieldDto.Amenities?.Select(fa => new FieldAmenity
            {
                AmenityName = fa.AmenityName,
                Description = fa.Description,
                IconUrl = fa.IconUrl
            }).ToList() ?? new List<FieldAmenity>();

            if (field.FieldImages?.Any() == true)
                _context.FieldImages.RemoveRange(field.FieldImages);

            if (updateFieldDto.Images?.Count > 50)
                throw new ArgumentException("Maximum 50 images allowed");

            field.FieldImages = updateFieldDto.Images?.Select(fi => new FieldImage
            {
                ImageUrl = fi.ImageUrl,
                PublicId = fi.PublicId,
                Thumbnail = fi.Thumbnail,
                IsPrimary = fi.IsPrimary,
                UploadedAt = DateTime.UtcNow
            }).ToList() ?? new List<FieldImage>();

            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"field_{fieldId}_none");
            await _cache.RemoveAsync($"field_{fieldId}_subfields");
            await _cache.RemoveAsync($"field_{fieldId}_services");
            await _cache.RemoveAsync($"field_{fieldId}_amenities");
            await _cache.RemoveAsync($"field_{fieldId}_images");

            return await GetFieldByIdAsync(fieldId, null);
        }

        public async Task DeleteFieldAsync(ClaimsPrincipal user, int fieldId)
        {
            var ownerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var field = await _context.Fields
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.Bookings)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null && f.OwnerId == ownerId);
            if (field == null)
                throw new UnauthorizedAccessException("Field not found or user is not the owner");

            var hasActiveBookings = field.SubFields.Any(sf => sf.Bookings.Any(b => b.Status != "Cancelled" && b.DeletedAt == null));
            if (hasActiveBookings)
                throw new ArgumentException("Cannot delete field with active bookings");

            field.Status = "Deleted";
            field.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"field_{fieldId}_none");
            await _cache.RemoveAsync($"field_{fieldId}_subfields");
            await _cache.RemoveAsync($"field_{fieldId}_services");
            await _cache.RemoveAsync($"field_{fieldId}_amenities");
            await _cache.RemoveAsync($"field_{fieldId}_images");
        }

        public async Task<List<AvailabilityResponseDto>> GetFieldAvailabilityAsync(int fieldId, AvailabilityFilterDto filterDto)
        {
            var field = await _context.Fields
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.PricingRules)
                .Include(f => f.SubFields)
                .ThenInclude(sf => sf.Bookings)
                .ThenInclude(b => b.TimeSlots)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null);
            if (field == null)
                throw new ArgumentException("Field not found");

            var dayOfWeek = filterDto.Date.DayOfWeek.ToString();
            var subFields = filterDto.SubFieldId.HasValue
                ? field.SubFields.Where(sf => sf.SubFieldId == filterDto.SubFieldId.Value).ToList()
                : field.SubFields;

            var result = new List<AvailabilityResponseDto>();
            foreach (var subField in subFields)
            {
                var slots = new List<AvailableSlotDto>();
                var start = filterDto.StartTime != null ? TimeSpan.Parse(filterDto.StartTime) : subField.OpenTime;
                var end = filterDto.EndTime != null ? TimeSpan.Parse(filterDto.EndTime) : subField.CloseTime;

                for (var time = start; time < end; time = time.Add(TimeSpan.FromMinutes(30)))
                {
                    var slotEnd = time.Add(TimeSpan.FromMinutes(30));
                    var price = subField.PricingRules
                        .Where(pr => pr.AppliesToDays.Contains(dayOfWeek))
                        .SelectMany(pr => pr.TimeSlots)
                        .FirstOrDefault(ts => ts.StartTime <= time && ts.EndTime >= slotEnd)?.PricePerSlot
                        ?? subField.DefaultPricePerSlot;

                    var isBooked = subField.Bookings
                        .Where(b => b.BookingDate.Date == filterDto.Date.Date && b.Status != "Cancelled" && b.DeletedAt == null)
                        .Any(b => b.TimeSlots.Any(ts => ts.StartTime < slotEnd && ts.EndTime > time));

                    var isParentBooked = subField.Parent7aSideId.HasValue && field.SubFields
                        .Where(sf => sf.SubFieldId == subField.Parent7aSideId)
                        .SelectMany(sf => sf.Bookings)
                        .Where(b => b.BookingDate.Date == filterDto.Date.Date && b.Status != "Cancelled" && b.DeletedAt == null)
                        .Any(b => b.TimeSlots.Any(ts => ts.StartTime < slotEnd && ts.EndTime > time));

                    var isChildBooked = subField.Child5aSideIds != null && subField.Child5aSideIds.Any(id => field.SubFields
                        .Where(sf => sf.SubFieldId == id)
                        .SelectMany(sf => sf.Bookings)
                        .Where(b => b.BookingDate.Date == filterDto.Date.Date && b.Status != "Cancelled" && b.DeletedAt == null)
                        .Any(b => b.TimeSlots.Any(ts => ts.StartTime < slotEnd && ts.EndTime > time)));

                    var isAvailable = !isBooked && !isParentBooked && !isChildBooked;
                    var unavailableReason = !isAvailable
                        ? (isBooked ? "Subfield booked" : isParentBooked ? "Parent subfield booked" : "Child subfield booked")
                        : null;

                    slots.Add(new AvailableSlotDto
                    {
                        StartTime = time.ToString(@"HH\:mm"),
                        EndTime = slotEnd.ToString(@"HH\:mm"),
                        PricePerSlot = price,
                        IsAvailable = isAvailable,
                        UnavailableReason = unavailableReason
                    });
                }

                result.Add(new AvailabilityResponseDto
                {
                    SubFieldId = subField.SubFieldId,
                    SubFieldName = subField.SubFieldName,
                    AvailableSlots = slots
                });
            }

            return result;
        }

        public async Task<(List<ReviewResponseDto> Data, int Total, int Page, int PageSize)> GetFieldReviewsAsync(int fieldId, ReviewFilterDto filterDto)
        {
            var field = await _context.Fields.FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null);
            if (field == null)
                throw new ArgumentException("Field not found");

            var query = _context.Reviews
                .Where(r => r.FieldId == fieldId && r.IsVisible)
                .Include(r => r.User)
                .AsQueryable();

            if (filterDto.MinRating.HasValue)
                query = query.Where(r => r.Rating >= filterDto.MinRating);

            if (filterDto.SortBy == "rating")
                query = filterDto.SortOrder == "desc" ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating);
            else
                query = filterDto.SortOrder == "desc" ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt);

            var total = await query.CountAsync();
            var data = await query
                .Skip((filterDto.Page - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .Select(r => new ReviewResponseDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    FullName = r.User.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    OwnerReply = r.OwnerReply,
                    ReplyDate = r.ReplyDate
                }).ToListAsync();

            return (data, total, filterDto.Page, filterDto.PageSize);
        }

        public async Task<(List<BookingResponseDto> Data, int Total, int Page, int PageSize)> GetFieldBookingsAsync(ClaimsPrincipal user, int fieldId, BookingFilterDto filterDto)
        {
            var ownerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var field = await _context.Fields
                .FirstOrDefaultAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null && f.OwnerId == ownerId);
            if (field == null)
                throw new UnauthorizedAccessException("Field not found or user is not the owner");

            var query = _context.Bookings
                .Where(b => b.SubField.FieldId == fieldId && b.DeletedAt == null)
                .Include(b => b.SubField)
                .Include(b => b.User)
                .Include(b => b.TimeSlots)
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.FieldService)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterDto.Status))
                query = query.Where(b => b.Status == filterDto.Status);
            if (filterDto.StartDate.HasValue)
                query = query.Where(b => b.BookingDate >= filterDto.StartDate);
            if (filterDto.EndDate.HasValue)
            {
                var endDateTimeMax = filterDto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.BookingDate <= endDateTimeMax);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((filterDto.Page - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .Select(b => new BookingResponseDto
                {
                    BookingId = b.BookingId,
                    SubFieldId = b.SubFieldId,
                    SubFieldName = b.SubField.SubFieldName,
                    UserId = b.UserId,
                    FullName = b.User.FullName,
                    BookingDate = b.BookingDate,
                    TimeSlots = b.TimeSlots.Select(ts => new BookingTimeSlotResponseDto
                    {
                        StartTime = ts.StartTime.ToString(@"HH\:mm"),
                        EndTime = ts.EndTime.ToString(@"HH\:mm"),
                        Price = ts.Price
                    }).ToList(),
                    Services = b.BookingServices.Select(bs => new BookingServiceResponseDto
                    {
                        BookingServiceId = bs.BookingServiceId,
                        FieldServiceId = bs.FieldServiceId,
                        ServiceName = bs.FieldService.ServiceName,
                        Quantity = bs.Quantity,
                        Price = bs.Price,
                        Description = bs.Description
                    }).ToList(),
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    CreatedAt = b.CreatedAt
                }).ToListAsync();

            return (data, total, filterDto.Page, filterDto.PageSize);
        }
    }
}