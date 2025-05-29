using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using api.Models;
using api.Dtos.Field;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using api.Utils;
using api.Interfaces;

namespace api.Services
{
    public class FieldService : IFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeocodingService _geocodingService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<FieldService> _logger;
        private readonly IAuthService _authService;

        public FieldService(
            IUnitOfWork unitOfWork,
            IGeocodingService geocodingService,
            ICloudinaryService cloudinaryService,
            IDistributedCache cache,
            ILogger<FieldService> logger,
            IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _geocodingService = geocodingService;
            _cloudinaryService = cloudinaryService;
            _cache = cache;
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Lấy danh sách sân với các bộ lọc và sắp xếp.
        /// </summary>
        /// <param name="filter">Bộ lọc tìm kiếm sân.</param>
        /// <returns>Danh sách sân phân trang.</returns>
        public async Task<PagedResult<FieldResponseDto>> GetFieldsAsync(FieldFilterDto filter)
        {
            _logger.LogInformation("Lấy danh sách sân với bộ lọc: {@Filter}", filter);

            var cacheKey = $"fields_{filter.Page}_{filter.PageSize}_{filter.City}_{filter.District}_{filter.SportId}_{filter.Search}_{filter.Latitude}_{filter.Longitude}_{filter.Radius}_{filter.MinPrice}_{filter.MaxPrice}_{filter.SortBy}_{filter.SortOrder}";
            var cachedResult = await _cache.GetRecordAsync<PagedResult<FieldResponseDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Trả về danh sách sân từ cache.");
                return cachedResult;
            }

            try
            {
                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.Status != "Deleted" && f.DeletedAt == null);

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(filter.City))
                    query = query.Where(f => f.City == filter.City);
                if (!string.IsNullOrEmpty(filter.District))
                    query = query.Where(f => f.District == filter.District);
                if (filter.SportId.HasValue)
                    query = query.Where(f => f.SportId == filter.SportId);
                if (!string.IsNullOrEmpty(filter.Search))
                    query = query.Where(f => f.FieldName.Contains(filter.Search) || f.Address.Contains(filter.Search));
                if (filter.MinPrice.HasValue)
                    query = query.Where(f => f.SubFields.Any(sf => sf.DefaultPricePerSlot >= filter.MinPrice || sf.PricingRules.Any(pr => pr.TimeSlots.Any(ts => ts.PricePerSlot >= filter.MinPrice))));
                if (filter.MaxPrice.HasValue)
                    query = query.Where(f => f.SubFields.Any(sf => sf.DefaultPricePerSlot <= filter.MaxPrice || sf.PricingRules.Any(pr => pr.TimeSlots.Any(ts => ts.PricePerSlot <= filter.MaxPrice))));
                if (filter.Latitude.HasValue && filter.Longitude.HasValue)
                {
                    query = query.Where(f => CommonUtils.CalculateDistance(f.Latitude, f.Longitude, filter.Latitude.Value, filter.Longitude.Value) <= filter.Radius);
                }

                // Sắp xếp
                query = filter.SortBy switch
                {
                    "averageRating" => filter.SortOrder == "asc" ? query.OrderBy(f => f.AverageRating) : query.OrderByDescending(f => f.AverageRating),
                    "distance" when filter.Latitude.HasValue && filter.Longitude.HasValue => filter.SortOrder == "asc"
                        ? query.OrderBy(f => CommonUtils.CalculateDistance(f.Latitude, f.Longitude, filter.Latitude.Value, filter.Longitude.Value))
                        : query.OrderByDescending(f => CommonUtils.CalculateDistance(f.Latitude, f.Longitude, filter.Latitude.Value, filter.Longitude.Value)),
                    "price" => filter.SortOrder == "asc"
                        ? query.OrderBy(f => f.SubFields.Min(sf => sf.DefaultPricePerSlot))
                        : query.OrderByDescending(f => f.SubFields.Max(sf => sf.DefaultPricePerSlot)),
                    _ => filter.SortOrder == "asc" ? query.OrderBy(f => f.FieldId) : query.OrderByDescending(f => f.FieldId)
                };

                // Tính tổng số bản ghi
                var total = await query.CountAsync();

                // Lấy dữ liệu phân trang
                var pagedFields = await query
                    .Include(f => f.SubFields).ThenInclude(sf => sf.PricingRules).ThenInclude(pr => pr.TimeSlots)
                    .Include(f => f.FieldServices)
                    .Include(f => f.FieldAmenities)
                    .Include(f => f.FieldImages)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
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
                        OpenTime = f.OpenTime.ToString(@"hh\:mm"),
                        CloseTime = f.CloseTime.ToString(@"hh\:mm"),
                        AverageRating = f.AverageRating,
                        SportId = f.SportId,
                        Distance = filter.Latitude.HasValue && filter.Longitude.HasValue
                            ? CommonUtils.CalculateDistance(f.Latitude, f.Longitude, filter.Latitude.Value, filter.Longitude.Value)
                            : null,
                        MinPricePerSlot = f.SubFields.Any() ? f.SubFields.Min(sf => sf.DefaultPricePerSlot) : 0,
                        MaxPricePerSlot = f.SubFields.Any() ? f.SubFields.Max(sf => sf.DefaultPricePerSlot) : 0,
                        SubFields = f.SubFields.Select(sf => new SubFieldResponseDto
                        {
                            SubFieldId = sf.SubFieldId,
                            SubFieldName = sf.SubFieldName,
                            FieldType = sf.FieldType,
                            Description = sf.Description,
                            Status = sf.Status,
                            Capacity = sf.Capacity,
                            OpenTime = sf.OpenTime.ToString(@"hh\:mm"),
                            CloseTime = sf.CloseTime.ToString(@"hh\:mm"),
                            DefaultPricePerSlot = sf.DefaultPricePerSlot,
                            PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                            {
                                PricingRuleId = pr.PricingRuleId,
                                AppliesToDays = pr.AppliesToDays,
                                TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                                {
                                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
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
                            IsPrimary = fi.IsPrimary,
                            UploadedAt = fi.UploadedAt
                        }).ToList()
                    })
                    .ToListAsync();

                var result = new PagedResult<FieldResponseDto>
                {
                    Data = pagedFields,
                    Total = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                await _cache.SetRecordAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Lưu danh sách sân vào cache.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sân với bộ lọc: {@Filter}. StackTrace: {StackTrace}", filter, ex.StackTrace);
                throw new InvalidOperationException("Không thể lấy danh sách sân: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một sân theo ID.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="include">Danh sách dữ liệu liên quan cần bao gồm.</param>
        /// <returns>Thông tin sân.</returns>
        public async Task<FieldResponseDto> GetFieldByIdAsync(int fieldId, string include)
        {
            _logger.LogInformation("Lấy thông tin sân với ID: {FieldId}, include: {Include}", fieldId, include);

            var cacheKey = $"field_{fieldId}_{include}";
            var cachedField = await _cache.GetRecordAsync<FieldResponseDto>(cacheKey);
            if (cachedField != null)
            {
                _logger.LogInformation("Trả về thông tin sân từ cache.");
                return cachedField;
            }

            try
            {
                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null);

                var field = await query
                    .Include(f => f.SubFields).ThenInclude(sf => sf.PricingRules).ThenInclude(pr => pr.TimeSlots)
                    .Include(f => f.FieldServices)
                    .Include(f => f.FieldAmenities)
                    .Include(f => f.FieldImages)
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
                        OpenTime = f.OpenTime.ToString(@"hh\:mm"),
                        CloseTime = f.CloseTime.ToString(@"hh\:mm"),
                        AverageRating = f.AverageRating,
                        SportId = f.SportId,
                        Distance = null, // Không có latitude/longitude trong filter
                        MinPricePerSlot = f.SubFields.Any() ? f.SubFields.Min(sf => sf.DefaultPricePerSlot) : 0,
                        MaxPricePerSlot = f.SubFields.Any() ? f.SubFields.Max(sf => sf.DefaultPricePerSlot) : 0,
                        SubFields = f.SubFields.Select(sf => new SubFieldResponseDto
                        {
                            SubFieldId = sf.SubFieldId,
                            SubFieldName = sf.SubFieldName,
                            FieldType = sf.FieldType,
                            Description = sf.Description,
                            Status = sf.Status,
                            Capacity = sf.Capacity,
                            OpenTime = sf.OpenTime.ToString(@"hh\:mm"),
                            CloseTime = sf.CloseTime.ToString(@"hh\:mm"),
                            DefaultPricePerSlot = sf.DefaultPricePerSlot,
                            PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                            {
                                PricingRuleId = pr.PricingRuleId,
                                AppliesToDays = pr.AppliesToDays,
                                TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                                {
                                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
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
                            IsPrimary = fi.IsPrimary,
                            UploadedAt = fi.UploadedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    _logger.LogWarning("Sân với ID {FieldId} không tồn tại.", fieldId);
                    throw new KeyNotFoundException("Sân không tồn tại hoặc đã bị xóa.");
                }

                await _cache.SetRecordAsync(cacheKey, field, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Lưu thông tin sân vào cache.");

                return field;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể lấy thông tin sân: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Xác thực địa chỉ của sân.
        /// </summary>
        /// <param name="dto">Thông tin địa chỉ cần xác thực.</param>
        /// <returns>Kết quả xác thực địa chỉ.</returns>
        public async Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto dto)
        {
            _logger.LogInformation("Xác thực địa chỉ: {@Dto}", dto);

            var cacheKey = $"address_{dto.FieldName}_{dto.Address}_{dto.City}_{dto.District}";
            var cachedResult = await _cache.GetRecordAsync<ValidateAddressResponseDto>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Trả về kết quả xác thực địa chỉ từ cache.");
                return cachedResult;
            }

            try
            {
                var result = await _geocodingService.ValidateAddressAsync(dto);
                await _cache.SetRecordAsync(cacheKey, result, TimeSpan.FromHours(24));
                _logger.LogInformation("Lưu kết quả xác thực địa chỉ vào cache.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực địa chỉ. StackTrace: {StackTrace}", ex.StackTrace);
                throw new InvalidOperationException("Lỗi khi xác thực địa chỉ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Tạo mới một sân.
        /// </summary>
        /// <param name="dto">Thông tin sân cần tạo.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Thông tin sân vừa tạo.</returns>
        public async Task<FieldResponseDto> CreateFieldAsync(CreateFieldDto dto, string token)
        {
            _logger.LogInformation("Tạo sân mới: {@Dto}", dto);

            try
            {
                // Lấy Account từ token
                var principal = await GetClaimsPrincipalFromTokenAsync(token);
                var account = await _authService.GetCurrentUserAsync(principal);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                var addressValidation = await ValidateAddressAsync(new ValidateAddressDto
                {
                    FieldName = dto.FieldName,
                    Address = dto.Address,
                    City = dto.City,
                    District = dto.District
                });

                if (!addressValidation.IsValid)
                {
                    throw new InvalidOperationException("Địa chỉ không hợp lệ.");
                }

                // Sử dụng execution strategy của DbContext
                var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
                    try
                    {
                        var field = new Field
                        {
                            FieldName = dto.FieldName,
                            Description = dto.Description,
                            Address = dto.Address,
                            City = dto.City,
                            District = dto.District,
                            OpenTime = TimeSpan.Parse(dto.OpenTime),
                            CloseTime = TimeSpan.Parse(dto.CloseTime),
                            SportId = dto.SportId,
                            OwnerId = owner.OwnerId,
                            Latitude = addressValidation.Latitude,
                            Longitude = addressValidation.Longitude,
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow,
                            SubFields = dto.SubFields.Select(sf => new SubField
                            {
                                SubFieldName = sf.SubFieldName,
                                FieldType = sf.FieldType,
                                Description = sf.Description,
                                Capacity = sf.Capacity,
                                OpenTime = TimeSpan.Parse(sf.OpenTime),
                                CloseTime = TimeSpan.Parse(sf.CloseTime),
                                DefaultPricePerSlot = sf.DefaultPricePerSlot,
                                Parent7aSideId = sf.Parent7aSideId,
                                Child5aSideIds = sf.Child5aSideIds,
                                PricingRules = sf.PricingRules.Select(pr => new PricingRule
                                {
                                    AppliesToDays = pr.AppliesToDays,
                                    TimeSlots = pr.TimeSlots.Select(ts => new TimeSlot
                                    {
                                        StartTime = TimeSpan.Parse(ts.StartTime),
                                        EndTime = TimeSpan.Parse(ts.EndTime),
                                        PricePerSlot = ts.PricePerSlot
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                            FieldServices = dto.Services.Select(s => new api.Models.FieldService
                            {
                                ServiceName = s.ServiceName,
                                Price = s.Price,
                                Description = s.Description,
                                IsActive = true
                            }).ToList(),
                            FieldAmenities = dto.Amenities.Select(a => new FieldAmenity
                            {
                                AmenityName = a.AmenityName,
                                Description = a.Description,
                                IconUrl = a.IconUrl
                            }).ToList(),
                            FieldImages = dto.Images.Select(i => new FieldImage
                            {
                                ImageUrl = i.ImageUrl,
                                PublicId = i.PublicId,
                                IsPrimary = i.IsPrimary,
                                UploadedAt = DateTime.UtcNow
                            }).ToList()
                        };

                        // Kiểm tra ràng buộc
                        if (field.SubFields.Count > 10 || field.FieldServices.Count > 50 || field.FieldAmenities.Count > 50 || field.FieldImages.Count > 50)
                        {
                            throw new InvalidOperationException("Vượt quá số lượng tối đa cho subfields, services, amenities hoặc images.");
                        }

                        await _unitOfWork.Repository<Field>().AddAsync(field);
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var result = new FieldResponseDto
                        {
                            FieldId = field.FieldId,
                            FieldName = field.FieldName,
                            Description = field.Description,
                            Address = field.Address,
                            City = field.City,
                            District = field.District,
                            Latitude = field.Latitude,
                            Longitude = field.Longitude,
                            OpenTime = field.OpenTime.ToString(@"hh\:mm"),
                            CloseTime = field.CloseTime.ToString(@"hh\:mm"),
                            AverageRating = field.AverageRating,
                            SportId = field.SportId,
                            Distance = null,
                            MinPricePerSlot = field.SubFields.Any() ? field.SubFields.Min(sf => sf.DefaultPricePerSlot) : 0,
                            MaxPricePerSlot = field.SubFields.Any() ? field.SubFields.Max(sf => sf.DefaultPricePerSlot) : 0,
                            SubFields = field.SubFields.Select(sf => new SubFieldResponseDto
                            {
                                SubFieldId = sf.SubFieldId,
                                SubFieldName = sf.SubFieldName,
                                FieldType = sf.FieldType,
                                Description = sf.Description,
                                Status = sf.Status,
                                Capacity = sf.Capacity,
                                OpenTime = sf.OpenTime.ToString(@"hh\:mm"),
                                CloseTime = sf.CloseTime.ToString(@"hh\:mm"),
                                DefaultPricePerSlot = sf.DefaultPricePerSlot,
                                PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                                {
                                    PricingRuleId = pr.PricingRuleId,
                                    AppliesToDays = pr.AppliesToDays,
                                    TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                                    {
                                        StartTime = ts.StartTime.ToString(@"hh\:mm"),
                                        EndTime = ts.EndTime.ToString(@"hh\:mm"),
                                        PricePerSlot = ts.PricePerSlot
                                    }).ToList()
                                }).ToList(),
                                Parent7aSideId = sf.Parent7aSideId,
                                Child5aSideIds = sf.Child5aSideIds
                            }).ToList(),
                            Services = field.FieldServices.Select(fs => new FieldServiceResponseDto
                            {
                                FieldServiceId = fs.FieldServiceId,
                                ServiceName = fs.ServiceName,
                                Price = fs.Price,
                                Description = fs.Description,
                                IsActive = fs.IsActive
                            }).ToList(),
                            Amenities = field.FieldAmenities.Select(fa => new FieldAmenityResponseDto
                            {
                                FieldAmenityId = fa.FieldAmenityId,
                                AmenityName = fa.AmenityName,
                                Description = fa.Description,
                                IconUrl = fa.IconUrl
                            }).ToList(),
                            Images = field.FieldImages.Select(fi => new FieldImageResponseDto
                            {
                                FieldImageId = fi.FieldImageId,
                                ImageUrl = fi.ImageUrl,
                                PublicId = fi.PublicId,
                                IsPrimary = fi.IsPrimary,
                                UploadedAt = fi.UploadedAt
                            }).ToList()
                        };

                        await _cache.SetRecordAsync($"field_{field.FieldId}", result, TimeSpan.FromMinutes(5));
                        _logger.LogInformation("Tạo sân mới thành công với ID: {FieldId}", field.FieldId);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Lỗi khi tạo sân mới trong giao dịch. StackTrace: {StackTrace}", ex.StackTrace);
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sân mới. StackTrace: {StackTrace}", ex.StackTrace);
                throw new InvalidOperationException("Không thể tạo sân: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Tải lên hình ảnh cho sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="dto">Thông tin hình ảnh.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Thông tin hình ảnh vừa tải lên.</returns>
        public async Task<FieldImageResponseDto> UploadFieldImageAsync(int fieldId, UploadFieldImageDto dto, string token)
        {
            _logger.LogInformation("Tải lên hình ảnh cho sân với ID: {FieldId}", fieldId);

            try
            {
                // Lấy Account từ token
                var principal = await GetClaimsPrincipalFromTokenAsync(token);
                var account = await _authService.GetCurrentUserAsync(principal);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                var field = await _unitOfWork.Repository<Field>()
                    .FindSingleAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                if (field == null)
                {
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                var imageCount = await (await _unitOfWork.Repository<FieldImage>()
                    .FindAsQueryableAsync(i => i.FieldId == fieldId))
                    .CountAsync();
                if (imageCount >= 50)
                {
                    throw new InvalidOperationException("Số lượng hình ảnh đã đạt tối đa (50 hình ảnh).");
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(dto.Image);
                    var fieldImage = new FieldImage
                    {
                        FieldId = fieldId,
                        ImageUrl = uploadResult.Url,
                        PublicId = uploadResult.PublicId,
                        IsPrimary = dto.IsPrimary,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<FieldImage>().AddAsync(fieldImage);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var result = new FieldImageResponseDto
                    {
                        FieldImageId = fieldImage.FieldImageId,
                        ImageUrl = fieldImage.ImageUrl,
                        PublicId = fieldImage.PublicId,
                        IsPrimary = fieldImage.IsPrimary,
                        UploadedAt = fieldImage.UploadedAt
                    };

                    await _cache.RemoveAsync($"field_{fieldId}");
                    _logger.LogInformation("Tải lên hình ảnh thành công cho sân ID: {FieldId}", fieldId);

                    return result;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lên hình ảnh cho sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể tải lên hình ảnh: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cập nhật thông tin sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="dto">Thông tin cập nhật.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        public async Task<FieldResponseDto> UpdateFieldAsync(int fieldId, UpdateFieldDto dto, string token)
        {
            _logger.LogInformation("Cập nhật sân với ID: {FieldId}", fieldId);

            try
            {
                // Lấy Account từ token
                var principal = await GetClaimsPrincipalFromTokenAsync(token);
                var account = await _authService.GetCurrentUserAsync(principal);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                var field = await query
                    .Include(f => f.SubFields)
                    .Include(f => f.FieldServices)
                    .Include(f => f.FieldAmenities)
                    .Include(f => f.FieldImages)
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                var addressValidation = await ValidateAddressAsync(new ValidateAddressDto
                {
                    FieldName = dto.FieldName,
                    Address = dto.Address,
                    City = dto.City,
                    District = dto.District
                });

                if (!addressValidation.IsValid)
                {
                    throw new InvalidOperationException("Địa chỉ không hợp lệ.");
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    field.FieldName = dto.FieldName;
                    field.Description = dto.Description;
                    field.Address = dto.Address;
                    field.City = dto.City;
                    field.District = dto.District;
                    field.OpenTime = TimeSpan.Parse(dto.OpenTime);
                    field.CloseTime = TimeSpan.Parse(dto.CloseTime);
                    field.SportId = dto.SportId;
                    field.Latitude = addressValidation.Latitude;
                    field.Longitude = addressValidation.Longitude;
                    field.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Repository<Field>().Update(field);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var result = MapToFieldResponseDto(field, null, null);
                    await _cache.SetRecordAsync($"field_{field.FieldId}", result, TimeSpan.FromMinutes(5));
                    await _cache.RemoveAsync($"fields_*");
                    _logger.LogInformation("Cập nhật sân thành công với ID: {FieldId}", fieldId);

                    return result;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể cập nhật sân: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Xóa mềm một sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Thông tin sân đã xóa.</returns>
        public async Task<DeleteFieldResponseDto> DeleteFieldAsync(int fieldId, string token)
        {
            _logger.LogInformation("Xóa sân với ID: {FieldId}", fieldId);

            try
            {
                // Lấy Account từ token
                var principal = await GetClaimsPrincipalFromTokenAsync(token);
                var account = await _authService.GetCurrentUserAsync(principal);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                var field = await query
                    .Include(f => f.SubFields).ThenInclude(sf => sf.Bookings)
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                var hasActiveBookings = field.SubFields.Any(sf => sf.Bookings.Any(b => (b.Status == "Confirmed" || b.Status == "Pending") && b.DeletedAt == null));
                if (hasActiveBookings)
                {
                    throw new InvalidOperationException("Không thể xóa sân vì còn đặt sân đang hoạt động.");
                }

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    field.Status = "Deleted";
                    field.DeletedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Field>().Update(field);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var result = new DeleteFieldResponseDto
                    {
                        FieldId = field.FieldId,
                        Status = field.Status,
                        DeletedAt = field.DeletedAt.Value,
                        Message = "Sân đã được xóa thành công."
                    };

                    await _cache.RemoveAsync($"field_{fieldId}");
                    await _cache.RemoveAsync($"fields_*");
                    _logger.LogInformation("Xóa sân thành công với ID: {FieldId}", fieldId);

                    return result;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể xóa sân: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lấy danh sách khung giờ trống của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc khung giờ.</param>
        /// <returns>Danh sách khung giờ trống.</returns>
        public async Task<List<AvailabilityResponseDto>> GetFieldAvailabilityAsync(int fieldId, AvailabilityFilterDto filter)
        {
            _logger.LogInformation("Lấy khung giờ trống cho sân ID: {FieldId}, bộ lọc: {@Filter}", fieldId, filter);

            try
            {
                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.Status != "Deleted" && f.DeletedAt == null);
                var field = await query
                    .Include(f => f.SubFields).ThenInclude(sf => sf.PricingRules).ThenInclude(pr => pr.TimeSlots)
                    .Include(f => f.SubFields).ThenInclude(sf => sf.Bookings)
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    throw new KeyNotFoundException("Sân không tồn tại hoặc đã bị xóa.");
                }

                var subFields = filter.SubFieldId.HasValue
                    ? field.SubFields.Where(sf => sf.SubFieldId == filter.SubFieldId.Value).ToList()
                    : field.SubFields.ToList();

                var result = subFields.Select(sf => new AvailabilityResponseDto
                {
                    SubFieldId = sf.SubFieldId,
                    SubFieldName = sf.SubFieldName,
                    AvailableSlots = GenerateAvailableSlots(sf, filter)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy khung giờ trống cho sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể lấy khung giờ trống: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lấy danh sách đánh giá của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc đánh giá.</param>
        /// <returns>Danh sách đánh giá phân trang.</returns>
        public async Task<PagedResult<ReviewResponseDto>> GetFieldReviewsAsync(int fieldId, ReviewFilterDto filter)
        {
            _logger.LogInformation("Lấy đánh giá cho sân ID: {FieldId}, bộ lọc: {@Filter}", fieldId, filter);

            try
            {
                var query = await _unitOfWork.Repository<Review>()
                    .FindAsQueryableAsync(r => r.FieldId == fieldId && r.IsVisible && r.Field.DeletedAt == null && r.Field.Status != "Deleted");

                if (filter.MinRating.HasValue)
                    query = query.Where(r => r.Rating >= filter.MinRating.Value);

                query = filter.SortBy == "rating"
                    ? filter.SortOrder == "asc" ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating)
                    : filter.SortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt);

                var total = await query.CountAsync();
                var pagedReviews = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
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
                    })
                    .ToListAsync();

                return new PagedResult<ReviewResponseDto>
                {
                    Data = pagedReviews,
                    Total = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đánh giá cho sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể lấy đánh giá: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lấy danh sách đặt sân của một sân (chỉ dành cho Owner).
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc đặt sân.</param>
        /// <param name="token">Token xác thực.</param>
        /// <returns>Danh sách đặt sân phân trang.</returns>
        public async Task<PagedResult<BookingResponseDto>> GetFieldBookingsAsync(int fieldId, BookingFilterDto filter, string token)
        {
            _logger.LogInformation("Lấy danh sách đặt sân cho sân ID: {FieldId}, bộ lọc: {@Filter}", fieldId, filter);

            try
            {
                var principal = await GetClaimsPrincipalFromTokenAsync(token);
                var account = await _authService.GetCurrentUserAsync(principal);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");

                var field = await _unitOfWork.Repository<Field>()
                    .FindSingleAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                if (field == null)
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");

                var query = await _unitOfWork.Repository<Booking>()
                    .FindAsQueryableAsync(b => b.SubField.FieldId == fieldId && b.DeletedAt == null);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(b => b.Status == filter.Status);
                if (filter.StartDate.HasValue)
                    query = query.Where(b => b.BookingDate >= filter.StartDate.Value);
                if (filter.EndDate.HasValue)
                    query = query.Where(b => b.BookingDate <= filter.EndDate.Value);

                var total = await query.CountAsync();

                // Thêm OrderBy trước Skip/Take
                query = query.OrderBy(b => b.BookingId);

                var pagedBookings = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Include(b => b.SubField)
                    .Include(b => b.User)
                    .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
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
                            StartTime = ts.StartTime.ToString(@"hh\.mm"),
                            EndTime = ts.EndTime.ToString(@"hh\.mm"),
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

                return new PagedResult<BookingResponseDto>
                {
                    Data = pagedBookings,
                    Total = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đặt sân cho sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw new InvalidOperationException("Không thể lấy danh sách đặt sân: " + ex.Message, ex);
            }
        }

        // Helper methods
        private FieldResponseDto MapToFieldResponseDto(Field field, double? latitude, double? longitude)
        {
            return new FieldResponseDto
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                Description = field.Description,
                Address = field.Address,
                City = field.City,
                District = field.District,
                Latitude = field.Latitude,
                Longitude = field.Longitude,
                OpenTime = field.OpenTime.ToString(@"hh\:mm"),
                CloseTime = field.CloseTime.ToString(@"hh\:mm"),
                AverageRating = field.AverageRating,
                SportId = field.SportId,
                Distance = latitude.HasValue && longitude.HasValue
                    ? CommonUtils.CalculateDistance(field.Latitude, field.Longitude, latitude.Value, longitude.Value)
                    : null,
                MinPricePerSlot = field.SubFields.Any() ? field.SubFields.Min(sf => sf.DefaultPricePerSlot) : 0,
                MaxPricePerSlot = field.SubFields.Any() ? field.SubFields.Max(sf => sf.DefaultPricePerSlot) : 0,
                SubFields = field.SubFields.Select(sf => new SubFieldResponseDto
                {
                    SubFieldId = sf.SubFieldId,
                    SubFieldName = sf.SubFieldName,
                    FieldType = sf.FieldType,
                    Description = sf.Description,
                    Status = sf.Status,
                    Capacity = sf.Capacity,
                    OpenTime = sf.OpenTime.ToString(@"hh\:mm"),
                    CloseTime = sf.CloseTime.ToString(@"hh\:mm"),
                    DefaultPricePerSlot = sf.DefaultPricePerSlot,
                    PricingRules = sf.PricingRules.Select(pr => new PricingRuleResponseDto
                    {
                        PricingRuleId = pr.PricingRuleId,
                        AppliesToDays = pr.AppliesToDays,
                        TimeSlots = pr.TimeSlots.Select(ts => new TimeSlotResponseDto
                        {
                            StartTime = ts.StartTime.ToString(@"hh\:mm"),
                            EndTime = ts.EndTime.ToString(@"hh\:mm"),
                            PricePerSlot = ts.PricePerSlot
                        }).ToList()
                    }).ToList(),
                    Parent7aSideId = sf.Parent7aSideId,
                    Child5aSideIds = sf.Child5aSideIds
                }).ToList(),
                Services = field.FieldServices.Select(fs => new FieldServiceResponseDto
                {
                    FieldServiceId = fs.FieldServiceId,
                    ServiceName = fs.ServiceName,
                    Price = fs.Price,
                    Description = fs.Description,
                    IsActive = fs.IsActive
                }).ToList(),
                Amenities = field.FieldAmenities.Select(fa => new FieldAmenityResponseDto
                {
                    FieldAmenityId = fa.FieldAmenityId,
                    AmenityName = fa.AmenityName,
                    Description = fa.Description,
                    IconUrl = fa.IconUrl
                }).ToList(),
                Images = field.FieldImages.Select(fi => new FieldImageResponseDto
                {
                    FieldImageId = fi.FieldImageId,
                    ImageUrl = fi.ImageUrl,
                    PublicId = fi.PublicId,
                    IsPrimary = fi.IsPrimary,
                    UploadedAt = fi.UploadedAt
                }).ToList()
            };
        }

        private List<AvailableSlotDto> GenerateAvailableSlots(SubField subField, AvailabilityFilterDto filter)
        {
            var slots = new List<AvailableSlotDto>();
            var startTime = filter.StartTime != null ? TimeSpan.Parse(filter.StartTime) : subField.OpenTime;
            var endTime = filter.EndTime != null ? TimeSpan.Parse(filter.EndTime) : subField.CloseTime;

            if (startTime < subField.OpenTime || endTime > subField.CloseTime)
            {
                throw new InvalidOperationException("Khung giờ yêu cầu nằm ngoài giờ hoạt động của sân.");
            }

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                var slotEnd = currentTime.Add(TimeSpan.FromMinutes(30));
                var isBooked = subField.Bookings.Any(b => b.BookingDate == filter.Date && b.TimeSlots.Any(ts => ts.StartTime <= currentTime && ts.EndTime > currentTime && ts.DeletedAt == null));
                var price = subField.PricingRules
                    .Where(pr => pr.AppliesToDays.Contains(filter.Date.DayOfWeek.ToString()))
                    .SelectMany(pr => pr.TimeSlots)
                    .FirstOrDefault(ts => ts.StartTime <= currentTime && ts.EndTime >= slotEnd)?.PricePerSlot ?? subField.DefaultPricePerSlot;

                slots.Add(new AvailableSlotDto
                {
                    StartTime = currentTime.ToString(@"hh\:mm"),
                    EndTime = slotEnd.ToString(@"hh\:mm"),
                    PricePerSlot = price,
                    IsAvailable = !isBooked,
                    UnavailableReason = isBooked ? "Khung giờ đã được đặt." : null
                });

                currentTime = slotEnd;
            }

            return slots;
        }

        private async Task<ClaimsPrincipal> GetClaimsPrincipalFromTokenAsync(string token)
        {
            var (isValid, role) = await _authService.VerifyTokenAsync(token);
            if (!isValid)
            {
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}