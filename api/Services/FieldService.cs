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
                await _cache.SetRecordAsync("fields_keys", new List<string> { cacheKey }, TimeSpan.FromHours(24));
                _logger.LogInformation("Lưu danh sách sân vào cache với key: {CacheKey}", cacheKey);

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
        public async Task<FieldResponseDto> CreateFieldAsync(CreateFieldDto dto, ClaimsPrincipal user)
        {
            _logger.LogInformation("Tạo sân mới: {@Dto}", dto);

            try
            {
                // Lấy Account từ token
                var account = await _authService.GetCurrentUserAsync(user);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                // Kiểm tra DTO và các điều kiện khác
                if (dto == null)
                {
                    _logger.LogError("DTO là null.");
                    throw new ArgumentNullException(nameof(dto), "Thông tin sân không được để trống.");
                }

                var sport = await _unitOfWork.Repository<Sport>()
                    .FindSingleAsync(s => s.SportId == dto.SportId && s.IsActive && s.DeletedAt == null);
                if (sport == null)
                {
                    _logger.LogWarning("SportId {SportId} không tồn tại hoặc không hoạt động.", dto.SportId);
                    throw new InvalidOperationException($"SportId {dto.SportId} không tồn tại hoặc không hoạt động.");
                }

                if (dto.SubFields == null || !dto.SubFields.Any())
                {
                    _logger.LogWarning("Danh sách SubFields không được để trống.");
                    throw new InvalidOperationException("Phải có ít nhất một SubField.");
                }

                foreach (var sf in dto.SubFields.Where(sf => sf.Parent7aSideId.HasValue))
                {
                    var parentExists = await _unitOfWork.Repository<SubField>()
                        .FindSingleAsync(s => s.SubFieldId == sf.Parent7aSideId);
                    if (parentExists == null)
                    {
                        _logger.LogWarning("Parent7aSideId {Parent7aSideId} không tồn tại.", sf.Parent7aSideId);
                        throw new InvalidOperationException($"Parent7aSideId {sf.Parent7aSideId} không tồn tại.");
                    }
                }

                if (!TimeSpan.TryParse(dto.OpenTime, out var openTime) || !TimeSpan.TryParse(dto.CloseTime, out var closeTime))
                {
                    _logger.LogWarning("Định dạng OpenTime hoặc CloseTime không hợp lệ: OpenTime={OpenTime}, CloseTime={CloseTime}", dto.OpenTime, dto.CloseTime);
                    throw new InvalidOperationException("Định dạng OpenTime hoặc CloseTime không hợp lệ.");
                }
                if (openTime >= closeTime)
                {
                    _logger.LogWarning("OpenTime phải nhỏ hơn CloseTime: OpenTime={OpenTime}, CloseTime={CloseTime}", dto.OpenTime, dto.CloseTime);
                    throw new InvalidOperationException("OpenTime phải nhỏ hơn CloseTime.");
                }

                foreach (var sf in dto.SubFields)
                {
                    if (!TimeSpan.TryParse(sf.OpenTime, out var sfOpenTime) || !TimeSpan.TryParse(sf.CloseTime, out var sfCloseTime))
                    {
                        _logger.LogWarning("Định dạng thời gian của SubField không hợp lệ: SubFieldName={SubFieldName}, OpenTime={OpenTime}, CloseTime={CloseTime}", sf.SubFieldName, sf.OpenTime, sf.CloseTime);
                        throw new InvalidOperationException($"Định dạng thời gian của sân con {sf.SubFieldName} không hợp lệ.");
                    }
                    if (sfOpenTime < openTime || sfCloseTime > closeTime)
                    {
                        _logger.LogWarning("Thời gian của SubField nằm ngoài thời gian của Field: SubFieldName={SubFieldName}, OpenTime={OpenTime}, CloseTime={CloseTime}", sf.SubFieldName, sf.OpenTime, sf.CloseTime);
                        throw new InvalidOperationException($"Thời gian hoạt động của sân con {sf.SubFieldName} phải nằm trong thời gian hoạt động của sân chính.");
                    }
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
                    _logger.LogWarning("Địa chỉ không hợp lệ: {@Address}", addressValidation);
                    throw new InvalidOperationException("Địa chỉ không hợp lệ.");
                }

                var strategy = _unitOfWork.CreateExecutionStrategy();
                int fieldId = await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync(); // Bắt đầu giao dịch
                    try
                    {
                        _logger.LogInformation("Bắt đầu lưu Field với {SubFieldCount} SubFields.", dto.SubFields.Count);

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
                            UpdatedAt = DateTime.UtcNow,
                            AverageRating = 0
                        };

                        _logger.LogInformation("Chuẩn bị lưu Field với FieldName: {FieldName}", field.FieldName);
                        await _unitOfWork.Repository<Field>().AddAsync(field);
                        await _unitOfWork.SaveChangesAsync(); // Lưu Field trước để có FieldId hợp lệ
                        _logger.LogInformation("Đã lưu Field với FieldId: {FieldId}", field.FieldId);

                        fieldId = field.FieldId;

                        if (dto.SubFields.Count > 10)
                        {
                            _logger.LogWarning("Số lượng SubFields vượt quá giới hạn: {SubFieldCount}", dto.SubFields.Count);
                            throw new InvalidOperationException("Số lượng sân con không được vượt quá 10.");
                        }

                        foreach (var subFieldDto in dto.SubFields)
                        {
                            var subField = new SubField
                            {
                                SubFieldName = subFieldDto.SubFieldName,
                                FieldType = subFieldDto.FieldType,
                                Description = subFieldDto.Description,
                                Capacity = subFieldDto.Capacity,
                                OpenTime = TimeSpan.Parse(subFieldDto.OpenTime),
                                CloseTime = TimeSpan.Parse(subFieldDto.CloseTime),
                                DefaultPricePerSlot = subFieldDto.DefaultPricePerSlot,
                                FieldId = field.FieldId,
                                Parent7aSideId = subFieldDto.Parent7aSideId,
                                Child5aSideIds = subFieldDto.Child5aSideIds ?? new List<int>(),
                                Status = "Active",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _logger.LogInformation("Chuẩn bị lưu SubField với SubFieldName: {SubFieldName}, FieldId: {FieldId}", subField.SubFieldName, subField.FieldId);
                            await _unitOfWork.Repository<SubField>().AddAsync(subField);
                            await _unitOfWork.SaveChangesAsync(); // Lưu SubField để có SubFieldId hợp lệ

                            foreach (var pricingRuleDto in subFieldDto.PricingRules)
                            {
                                var pricingRule = new PricingRule
                                {
                                    SubFieldId = subField.SubFieldId,
                                    AppliesToDays = pricingRuleDto.AppliesToDays,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _unitOfWork.Repository<PricingRule>().AddAsync(pricingRule);
                                await _unitOfWork.SaveChangesAsync(); // Lưu PricingRule để có PricingRuleId hợp lệ

                                foreach (var timeSlotDto in pricingRuleDto.TimeSlots)
                                {
                                    var timeSlot = new TimeSlot
                                    {
                                        PricingRuleId = pricingRule.PricingRuleId,
                                        StartTime = TimeSpan.Parse(timeSlotDto.StartTime),
                                        EndTime = TimeSpan.Parse(timeSlotDto.EndTime),
                                        PricePerSlot = timeSlotDto.PricePerSlot,
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow
                                    };

                                    await _unitOfWork.Repository<TimeSlot>().AddAsync(timeSlot);
                                }
                            }
                        }

                        if (dto.Services != null && dto.Services.Any())
                        {
                            if (dto.Services.Count > 50)
                            {
                                _logger.LogWarning("Số lượng Services vượt quá giới hạn: {ServiceCount}", dto.Services.Count);
                                throw new InvalidOperationException("Số lượng dịch vụ không được vượt quá 50.");
                            }

                            foreach (var serviceDto in dto.Services)
                            {
                                var service = new Models.FieldService
                                {
                                    FieldId = field.FieldId,
                                    ServiceName = serviceDto.ServiceName,
                                    Price = serviceDto.Price,
                                    Description = serviceDto.Description,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _unitOfWork.Repository<Models.FieldService>().AddAsync(service);
                            }
                        }

                        if (dto.Amenities != null && dto.Amenities.Any())
                        {
                            if (dto.Amenities.Count > 50)
                            {
                                _logger.LogWarning("Số lượng Amenities vượt quá giới hạn: {AmenityCount}", dto.Amenities.Count);
                                throw new InvalidOperationException("Số lượng tiện ích không được vượt quá 50.");
                            }

                            foreach (var amenityDto in dto.Amenities)
                            {
                                var amenity = new FieldAmenity
                                {
                                    FieldId = field.FieldId,
                                    AmenityName = amenityDto.AmenityName,
                                    Description = amenityDto.Description,
                                    IconUrl = amenityDto.IconUrl,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _unitOfWork.Repository<FieldAmenity>().AddAsync(amenity);
                            }
                        }

                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Đã lưu tất cả SubFields, Services, và Amenities cho FieldId: {FieldId}", field.FieldId);

                        if (dto.Images.Any())
                        {
                            foreach (var image in dto.Images)
                            {
                                if (image == null || image.Length == 0)
                                {
                                    _logger.LogWarning("Hình ảnh không hợp lệ trong danh sách Images.");
                                    throw new InvalidOperationException("Một hoặc nhiều hình ảnh không hợp lệ.");
                                }
                                var uploadDto = new UploadFieldImageDto { Image = image, IsPrimary = false };
                                await UploadFieldImageAsync(field.FieldId, uploadDto, user);
                            }
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync(); // Commit giao dịch
                        _logger.LogInformation("Commit transaction thành công cho FieldId: {FieldId}", field.FieldId);
                        return field.FieldId;
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Lỗi khi tạo sân mới trong giao dịch. StackTrace: {StackTrace}", ex.StackTrace);
                        throw new InvalidOperationException("Không thể tạo sân: " + ex.Message, ex);
                    }
                });

                // Lấy lại Field sau khi commit
                try
                {
                    var repository = _unitOfWork.Repository<Field>();
                    var query = await repository.FindAsQueryableAsync(f => f.FieldId == fieldId);
                    var savedField = await query
                        .Include(f => f.SubFields).ThenInclude(sf => sf.PricingRules).ThenInclude(pr => pr.TimeSlots)
                        .Include(f => f.FieldServices)
                        .Include(f => f.FieldAmenities)
                        .Include(f => f.FieldImages)
                        .FirstOrDefaultAsync();

                    if (savedField == null)
                    {
                        _logger.LogWarning("Không tìm thấy Field vừa tạo với FieldId: {FieldId}", fieldId);
                        throw new InvalidOperationException("Không tìm thấy sân vừa tạo.");
                    }

                    var result = MapToFieldResponseDto(savedField, null, null);
                    await _cache.SetRecordAsync($"field_{fieldId}", result, TimeSpan.FromMinutes(5));
                    _logger.LogInformation("Tạo sân mới thành công với ID: {FieldId}", fieldId);

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lấy lại thông tin sân hoặc lưu cache cho FieldId: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                    throw new InvalidOperationException("Tạo sân thành công nhưng lỗi khi lấy thông tin: " + ex.Message, ex);
                }
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
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin hình ảnh vừa tải lên.</returns>
        public async Task<FieldImageResponseDto> UploadFieldImageAsync(int fieldId, UploadFieldImageDto dto, ClaimsPrincipal user)
        {
            _logger.LogInformation("Tải lên hình ảnh cho sân với ID: {FieldId}", fieldId);

            try
            {
                // Kiểm tra DTO
                if (dto == null)
                {
                    _logger.LogWarning("DTO là null.");
                    throw new ArgumentNullException(nameof(dto), "Thông tin hình ảnh không được để trống.");
                }

                // Kiểm tra Image
                if (dto.Image == null || dto.Image.Length == 0)
                {
                    _logger.LogWarning("Tệp hình ảnh là null hoặc rỗng cho FieldId: {FieldId}", fieldId);
                    throw new ArgumentException("Tệp hình ảnh là bắt buộc.", nameof(dto.Image));
                }

                // Kiểm tra định dạng tệp
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Định dạng tệp không được hỗ trợ: {Extension} cho FieldId: {FieldId}", extension, fieldId);
                    throw new ArgumentException("Chỉ hỗ trợ các định dạng .jpg, .jpeg, .png, .gif.", nameof(dto.Image));
                }

                // Kiểm tra kích thước tệp
                if (dto.Image.Length > 10 * 1024 * 1024) // 10MB
                {
                    _logger.LogWarning("Kích thước tệp vượt quá 10MB: {FileSize} bytes cho FieldId: {FieldId}", dto.Image.Length, fieldId);
                    throw new ArgumentException("Kích thước tệp không được vượt quá 10MB.", nameof(dto.Image));
                }

                // Xác thực Owner
                var account = await _authService.GetCurrentUserAsync(user);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                // Kiểm tra Field
                var field = await _unitOfWork.Repository<Field>()
                    .FindSingleAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                if (field == null)
                {
                    _logger.LogWarning("Sân với FieldId {FieldId} không tồn tại hoặc không thuộc OwnerId {OwnerId}", fieldId, owner.OwnerId);
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                // Kiểm tra giới hạn hình ảnh
                var imageCount = await (await _unitOfWork.Repository<FieldImage>()
                    .FindAsQueryableAsync(i => i.FieldId == fieldId))
                    .CountAsync();
                if (imageCount >= 20)
                {
                    _logger.LogWarning("Số lượng hình ảnh đã đạt tối đa (20) cho FieldId: {FieldId}", fieldId);
                    throw new InvalidOperationException("Số lượng hình ảnh đã đạt tối đa (20 hình ảnh).");
                }

                // Tải lên hình ảnh
                _logger.LogInformation("Tải lên hình ảnh {FileName} ({FileSize} bytes) cho FieldId: {FieldId}", dto.Image.FileName, dto.Image.Length, fieldId);
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Lỗi xác thực khi tải lên hình ảnh cho sân ID: {FieldId}. Message: {Message}", fieldId, ex.Message);
                throw;
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
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        public async Task<FieldResponseDto> UpdateFieldAsync(int fieldId, UpdateFieldDto dto, ClaimsPrincipal user)
        {
            _logger.LogInformation("Cập nhật sân với ID: {FieldId}, dữ liệu: {@Dto}", fieldId, dto);

            try
            {
                // Lấy Account từ ClaimsPrincipal
                var account = await _authService.GetCurrentUserAsync(user);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                // Kiểm tra sân tồn tại và thuộc Owner
                var fieldQuery = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                var field = await fieldQuery
                    .Include(f => f.SubFields).ThenInclude(sf => sf.PricingRules).ThenInclude(pr => pr.TimeSlots)
                    .Include(f => f.FieldServices)
                    .Include(f => f.FieldAmenities)
                    .Include(f => f.FieldImages)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    _logger.LogWarning("Sân với FieldId {FieldId} không tồn tại hoặc không thuộc OwnerId {OwnerId}", fieldId, owner.OwnerId);
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                // Kiểm tra DTO
                if (dto == null)
                {
                    _logger.LogError("DTO là null.");
                    throw new ArgumentNullException(nameof(dto), "Thông tin sân không được để trống.");
                }

                var sport = await _unitOfWork.Repository<Sport>()
                    .FindSingleAsync(s => s.SportId == dto.SportId && s.IsActive && s.DeletedAt == null);
                if (sport == null)
                {
                    _logger.LogWarning("SportId {SportId} không tồn tại hoặc không hoạt động.", dto.SportId);
                    throw new InvalidOperationException($"SportId {dto.SportId} không tồn tại hoặc không hoạt động.");
                }

                if (dto.SubFields == null || !dto.SubFields.Any())
                {
                    _logger.LogWarning("Danh sách SubFields không được để trống.");
                    throw new InvalidOperationException("Phải có ít nhất một SubField.");
                }

                // Kiểm tra Parent7aSideId
                foreach (var sf in dto.SubFields.Where(sf => sf.Parent7aSideId.HasValue))
                {
                    var parentExists = await _unitOfWork.Repository<SubField>()
                        .FindSingleAsync(s => s.SubFieldId == sf.Parent7aSideId && s.FieldId == fieldId && s.DeletedAt == null);
                    if (parentExists == null)
                    {
                        _logger.LogWarning("Parent7aSideId {Parent7aSideId} không tồn tại hoặc không thuộc cùng sân.", sf.Parent7aSideId);
                        throw new InvalidOperationException($"Parent7aSideId {sf.Parent7aSideId} không tồn tại hoặc không thuộc cùng sân.");
                    }
                }

                // Kiểm tra thời gian
                if (!TimeSpan.TryParse(dto.OpenTime, out var openTime) || !TimeSpan.TryParse(dto.CloseTime, out var closeTime))
                {
                    _logger.LogWarning("Định dạng OpenTime hoặc CloseTime không hợp lệ: OpenTime={OpenTime}, CloseTime={CloseTime}", dto.OpenTime, dto.CloseTime);
                    throw new InvalidOperationException("Định dạng OpenTime hoặc CloseTime không hợp lệ.");
                }
                if (openTime >= closeTime)
                {
                    _logger.LogWarning("OpenTime phải nhỏ hơn CloseTime: OpenTime={OpenTime}, CloseTime={CloseTime}", dto.OpenTime, dto.CloseTime);
                    throw new InvalidOperationException("OpenTime phải nhỏ hơn CloseTime.");
                }

                foreach (var sf in dto.SubFields)
                {
                    if (!TimeSpan.TryParse(sf.OpenTime, out var sfOpenTime) || !TimeSpan.TryParse(sf.CloseTime, out var sfCloseTime))
                    {
                        _logger.LogWarning("Định dạng thời gian của SubField không hợp lệ: SubFieldName={SubFieldName}, OpenTime={OpenTime}, CloseTime={CloseTime}", sf.SubFieldName, sf.OpenTime, sf.CloseTime);
                        throw new InvalidOperationException($"Định dạng thời gian của sân con {sf.SubFieldName} không hợp lệ.");
                    }
                    if (sfOpenTime < openTime || sfCloseTime > closeTime)
                    {
                        _logger.LogWarning("Thời gian của SubField nằm ngoài thời gian của Field: SubFieldName={SubFieldName}, OpenTime={OpenTime}, CloseTime={CloseTime}", sf.SubFieldName, sf.OpenTime, sf.CloseTime);
                        throw new InvalidOperationException($"Thời gian hoạt động của sân con {sf.SubFieldName} phải nằm trong thời gian hoạt động của sân chính.");
                    }
                }

                // Kiểm tra địa chỉ
                var addressValidation = await ValidateAddressAsync(new ValidateAddressDto
                {
                    FieldName = dto.FieldName,
                    Address = dto.Address,
                    City = dto.City,
                    District = dto.District
                });

                if (!addressValidation.IsValid)
                {
                    _logger.LogWarning("Địa chỉ không hợp lệ: {@Address}", addressValidation);
                    throw new InvalidOperationException("Địa chỉ không hợp lệ.");
                }

                var strategy = _unitOfWork.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation("Bắt đầu cập nhật Field với ID: {FieldId}", fieldId);

                        // Cập nhật thông tin sân
                        var updatedField = new Field
                        {
                            FieldId = field.FieldId,
                            FieldName = dto.FieldName,
                            Description = dto.Description,
                            Address = dto.Address,
                            City = dto.City,
                            District = dto.District,
                            OpenTime = openTime,
                            CloseTime = closeTime,
                            SportId = dto.SportId,
                            OwnerId = field.OwnerId,
                            Latitude = addressValidation.Latitude,
                            Longitude = addressValidation.Longitude,
                            Status = field.Status,
                            CreatedAt = field.CreatedAt,
                            UpdatedAt = DateTime.UtcNow,
                            AverageRating = field.AverageRating,
                            DeletedAt = null
                        };
                        _unitOfWork.Repository<Field>().Update(updatedField);
                        await _unitOfWork.SaveChangesAsync(); // Lưu Field để đảm bảo trạng thái nhất quán

                        // Cập nhật SubFields
                        var existingSubFields = await (await _unitOfWork.Repository<SubField>()
                            .FindAsQueryableAsync(sf => sf.FieldId == fieldId && sf.DeletedAt == null))
                            .AsNoTracking()
                            .ToListAsync();

                        var subFieldDtos = dto.SubFields.ToList();
                        var subFieldsToRemove = existingSubFields
                            .Where(sf => !subFieldDtos.Any(dto => dto.SubFieldId.HasValue && dto.SubFieldId == sf.SubFieldId))
                            .ToList();
                        foreach (var sf in subFieldsToRemove)
                        {
                            sf.DeletedAt = DateTime.UtcNow;
                            _unitOfWork.Repository<SubField>().Update(sf);
                            _logger.LogInformation("Đánh dấu xóa SubField: {SubFieldName}, SubFieldId: {SubFieldId}", sf.SubFieldName, sf.SubFieldId);
                        }

                        foreach (var subFieldDto in subFieldDtos)
                        {
                            SubField subField;
                            if (subFieldDto.SubFieldId.HasValue)
                            {
                                subField = existingSubFields.FirstOrDefault(sf => sf.SubFieldId == subFieldDto.SubFieldId.Value);
                                if (subField == null)
                                {
                                    _logger.LogWarning("SubFieldId {SubFieldId} không tồn tại.", subFieldDto.SubFieldId);
                                    throw new InvalidOperationException($"SubFieldId {subFieldDto.SubFieldId} không tồn tại.");
                                }
                                subField.SubFieldName = subFieldDto.SubFieldName;
                                subField.FieldType = subFieldDto.FieldType;
                                subField.Description = subFieldDto.Description;
                                subField.Capacity = subFieldDto.Capacity;
                                subField.OpenTime = TimeSpan.Parse(subFieldDto.OpenTime);
                                subField.CloseTime = TimeSpan.Parse(subFieldDto.CloseTime);
                                subField.DefaultPricePerSlot = subFieldDto.DefaultPricePerSlot;
                                subField.Parent7aSideId = subFieldDto.Parent7aSideId;
                                subField.Child5aSideIds = subFieldDto.Child5aSideIds ?? new List<int>();
                                subField.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.Repository<SubField>().Update(subField);
                                _logger.LogInformation("Cập nhật SubField: {SubFieldName}, SubFieldId: {SubFieldId}", subField.SubFieldName, subField.SubFieldId);
                            }
                            else
                            {
                                subField = new SubField
                                {
                                    FieldId = fieldId,
                                    SubFieldName = subFieldDto.SubFieldName,
                                    FieldType = subFieldDto.FieldType,
                                    Description = subFieldDto.Description,
                                    Capacity = subFieldDto.Capacity,
                                    OpenTime = TimeSpan.Parse(subFieldDto.OpenTime),
                                    CloseTime = TimeSpan.Parse(subFieldDto.CloseTime),
                                    DefaultPricePerSlot = subFieldDto.DefaultPricePerSlot,
                                    Parent7aSideId = subFieldDto.Parent7aSideId,
                                    Child5aSideIds = subFieldDto.Child5aSideIds ?? new List<int>(),
                                    Status = "Active",
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _unitOfWork.Repository<SubField>().AddAsync(subField);
                                _logger.LogInformation("Thêm mới SubField: {SubFieldName}", subField.SubFieldName);
                            }
                        }

                        // Lưu SubFields để sinh SubFieldId
                        await _unitOfWork.SaveChangesAsync();

                        // Cập nhật PricingRules và TimeSlots
                        foreach (var subFieldDto in subFieldDtos)
                        {
                            // Lấy lại SubField từ DB để đảm bảo có ID
                            var subField = subFieldDto.SubFieldId.HasValue
                                ? await _unitOfWork.Repository<SubField>()
                                    .FindSingleAsync(sf => sf.SubFieldId == subFieldDto.SubFieldId.Value && sf.DeletedAt == null)
                                : await _unitOfWork.Repository<SubField>()
                                    .FindSingleAsync(sf => sf.FieldId == fieldId && sf.SubFieldName == subFieldDto.SubFieldName && sf.DeletedAt == null);

                            if (subField == null)
                            {
                                _logger.LogWarning("Không tìm thấy SubField vừa tạo/cập nhật: {SubFieldName}", subFieldDto.SubFieldName);
                                throw new InvalidOperationException($"Không tìm thấy SubField {subFieldDto.SubFieldName}.");
                            }

                            // Cập nhật PricingRules
                            var existingRules = await (await _unitOfWork.Repository<PricingRule>()
                                .FindAsQueryableAsync(pr => pr.SubFieldId == subField.SubFieldId && pr.DeletedAt == null))
                                .AsNoTracking()
                                .ToListAsync();
                            var ruleDtos = subFieldDto.PricingRules.ToList();

                            var rulesToRemove = existingRules
                                .Where(r => !ruleDtos.Any(dto => dto.PricingRuleId.HasValue && dto.PricingRuleId == r.PricingRuleId))
                                .ToList();
                            foreach (var rule in rulesToRemove)
                            {
                                rule.DeletedAt = DateTime.UtcNow;
                                _unitOfWork.Repository<PricingRule>().Update(rule);
                                _logger.LogInformation("Đánh dấu xóa PricingRule, PricingRuleId: {PricingRuleId}", rule.PricingRuleId);
                            }

                            foreach (var ruleDto in ruleDtos)
                            {
                                PricingRule pricingRule;
                                if (ruleDto.PricingRuleId.HasValue)
                                {
                                    pricingRule = existingRules.FirstOrDefault(r => r.PricingRuleId == ruleDto.PricingRuleId.Value);
                                    if (pricingRule == null)
                                    {
                                        _logger.LogWarning("PricingRuleId {PricingRuleId} không tồn tại.", ruleDto.PricingRuleId);
                                        throw new InvalidOperationException($"PricingRuleId {ruleDto.PricingRuleId} không tồn tại.");
                                    }
                                    pricingRule.AppliesToDays = ruleDto.AppliesToDays;
                                    pricingRule.UpdatedAt = DateTime.UtcNow;
                                    _unitOfWork.Repository<PricingRule>().Update(pricingRule);
                                    _logger.LogInformation("Cập nhật PricingRule, PricingRuleId: {PricingRuleId}", pricingRule.PricingRuleId);
                                }
                                else
                                {
                                    pricingRule = new PricingRule
                                    {
                                        SubFieldId = subField.SubFieldId,
                                        AppliesToDays = ruleDto.AppliesToDays,
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow
                                    };
                                    await _unitOfWork.Repository<PricingRule>().AddAsync(pricingRule);
                                    _logger.LogInformation("Thêm mới PricingRule cho SubFieldId: {SubFieldId}", subField.SubFieldId);
                                }
                            }

                            // Lưu PricingRules để sinh PricingRuleId
                            await _unitOfWork.SaveChangesAsync();

                            // Cập nhật TimeSlots
                            foreach (var ruleDto in ruleDtos)
                            {
                                // Lấy lại PricingRule từ DB để đảm bảo có ID
                                var pricingRule = ruleDto.PricingRuleId.HasValue
                                    ? await _unitOfWork.Repository<PricingRule>()
                                        .FindSingleAsync(pr => pr.PricingRuleId == ruleDto.PricingRuleId.Value && pr.DeletedAt == null)
                                    : await _unitOfWork.Repository<PricingRule>()
                                        .FindSingleAsync(pr => pr.SubFieldId == subField.SubFieldId && pr.AppliesToDays == ruleDto.AppliesToDays && pr.DeletedAt == null);

                                if (pricingRule == null)
                                {
                                    _logger.LogWarning("Không tìm thấy PricingRule vừa tạo/cập nhật cho SubFieldId: {SubFieldId}", subField.SubFieldId);
                                    throw new InvalidOperationException("Không tìm thấy PricingRule vừa tạo/cập nhật.");
                                }

                                var existingSlots = await (await _unitOfWork.Repository<TimeSlot>()
                                    .FindAsQueryableAsync(ts => ts.PricingRuleId == pricingRule.PricingRuleId && ts.DeletedAt == null))
                                    .AsNoTracking()
                                    .ToListAsync();
                                var slotDtos = ruleDto.TimeSlots.ToList();

                                var slotsToRemove = existingSlots
                                    .Where(s => !slotDtos.Any(dto => dto.TimeSlotId.HasValue && dto.TimeSlotId == s.TimeSlotId))
                                    .ToList();
                                foreach (var slot in slotsToRemove)
                                {
                                    slot.DeletedAt = DateTime.UtcNow;
                                    _unitOfWork.Repository<TimeSlot>().Update(slot);
                                    _logger.LogInformation("Đánh dấu xóa TimeSlot, TimeSlotId: {TimeSlotId}", slot.TimeSlotId);
                                }

                                foreach (var slotDto in slotDtos)
                                {
                                    TimeSlot timeSlot;
                                    if (slotDto.TimeSlotId.HasValue)
                                    {
                                        timeSlot = existingSlots.FirstOrDefault(s => s.TimeSlotId == slotDto.TimeSlotId.Value);
                                        if (timeSlot == null)
                                        {
                                            _logger.LogWarning("TimeSlotId {TimeSlotId} không tồn tại.", slotDto.TimeSlotId);
                                            throw new InvalidOperationException($"TimeSlotId {slotDto.TimeSlotId} không tồn tại.");
                                        }
                                        timeSlot.StartTime = TimeSpan.Parse(slotDto.StartTime);
                                        timeSlot.EndTime = TimeSpan.Parse(slotDto.EndTime);
                                        timeSlot.PricePerSlot = slotDto.PricePerSlot;
                                        timeSlot.UpdatedAt = DateTime.UtcNow;
                                        _unitOfWork.Repository<TimeSlot>().Update(timeSlot);
                                        _logger.LogInformation("Cập nhật TimeSlot, TimeSlotId: {TimeSlotId}", timeSlot.TimeSlotId);
                                    }
                                    else
                                    {
                                        timeSlot = new TimeSlot
                                        {
                                            PricingRuleId = pricingRule.PricingRuleId,
                                            StartTime = TimeSpan.Parse(slotDto.StartTime),
                                            EndTime = TimeSpan.Parse(slotDto.EndTime),
                                            PricePerSlot = slotDto.PricePerSlot,
                                            CreatedAt = DateTime.UtcNow,
                                            UpdatedAt = DateTime.UtcNow
                                        };
                                        await _unitOfWork.Repository<TimeSlot>().AddAsync(timeSlot);
                                        _logger.LogInformation("Thêm mới TimeSlot cho PricingRuleId: {PricingRuleId}", pricingRule.PricingRuleId);
                                    }
                                }
                            }
                        }

                        // Cập nhật Services
                        var existingServices = await (await _unitOfWork.Repository<Models.FieldService>()
                            .FindAsQueryableAsync(fs => fs.FieldId == fieldId && fs.DeletedAt == null))
                            .AsNoTracking()
                            .ToListAsync();
                        var serviceDtos = dto.Services.ToList();

                        var servicesToRemove = existingServices
                            .Where(fs => !serviceDtos.Any(dto => dto.FieldServiceId.HasValue && dto.FieldServiceId == fs.FieldServiceId))
                            .ToList();
                        foreach (var service in servicesToRemove)
                        {
                            service.DeletedAt = DateTime.UtcNow;
                            _unitOfWork.Repository<Models.FieldService>().Update(service);
                            _logger.LogInformation("Đánh dấu xóa FieldService: {ServiceName}, FieldServiceId: {FieldServiceId}", service.ServiceName, service.FieldServiceId);
                        }

                        foreach (var serviceDto in serviceDtos)
                        {
                            Models.FieldService service;
                            if (serviceDto.FieldServiceId.HasValue)
                            {
                                service = existingServices.FirstOrDefault(fs => fs.FieldServiceId == serviceDto.FieldServiceId.Value);
                                if (service == null)
                                {
                                    _logger.LogWarning("FieldServiceId {FieldServiceId} không tồn tại.", serviceDto.FieldServiceId);
                                    throw new InvalidOperationException($"FieldServiceId {serviceDto.FieldServiceId} không tồn tại.");
                                }
                                service.ServiceName = serviceDto.ServiceName;
                                service.Price = serviceDto.Price;
                                service.Description = serviceDto.Description;
                                service.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.Repository<Models.FieldService>().Update(service);
                                _logger.LogInformation("Cập nhật FieldService: {ServiceName}, FieldServiceId: {FieldServiceId}", service.ServiceName, service.FieldServiceId);
                            }
                            else
                            {
                                service = new Models.FieldService
                                {
                                    FieldId = fieldId,
                                    ServiceName = serviceDto.ServiceName,
                                    Price = serviceDto.Price,
                                    Description = serviceDto.Description,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _unitOfWork.Repository<Models.FieldService>().AddAsync(service);
                                _logger.LogInformation("Thêm mới FieldService: {ServiceName}", service.ServiceName);
                            }
                        }

                        // Lưu Services
                        await _unitOfWork.SaveChangesAsync();

                        // Cập nhật Amenities
                        var existingAmenities = await (await _unitOfWork.Repository<FieldAmenity>()
                            .FindAsQueryableAsync(fa => fa.FieldId == fieldId && fa.DeletedAt == null))
                            .AsNoTracking()
                            .ToListAsync();
                        var amenityDtos = dto.Amenities.ToList();

                        var amenitiesToRemove = existingAmenities
                            .Where(fa => !amenityDtos.Any(dto => dto.FieldAmenityId.HasValue && dto.FieldAmenityId == fa.FieldAmenityId))
                            .ToList();
                        foreach (var amenity in amenitiesToRemove)
                        {
                            amenity.DeletedAt = DateTime.UtcNow;
                            _unitOfWork.Repository<FieldAmenity>().Update(amenity);
                            _logger.LogInformation("Đánh dấu xóa FieldAmenity: {AmenityName}, FieldAmenityId: {FieldAmenityId}", amenity.AmenityName, amenity.FieldAmenityId);
                        }

                        foreach (var amenityDto in amenityDtos)
                        {
                            FieldAmenity amenity;
                            if (amenityDto.FieldAmenityId.HasValue)
                            {
                                amenity = existingAmenities.FirstOrDefault(fa => fa.FieldAmenityId == amenityDto.FieldAmenityId.Value);
                                if (amenity == null)
                                {
                                    _logger.LogWarning("FieldAmenityId {FieldAmenityId} không tồn tại.", amenityDto.FieldAmenityId);
                                    throw new InvalidOperationException($"FieldAmenityId {amenityDto.FieldAmenityId} không tồn tại.");
                                }
                                amenity.AmenityName = amenityDto.AmenityName;
                                amenity.Description = amenityDto.Description;
                                amenity.IconUrl = amenityDto.IconUrl;
                                amenity.UpdatedAt = DateTime.UtcNow;
                                _unitOfWork.Repository<FieldAmenity>().Update(amenity);
                                _logger.LogInformation("Cập nhật FieldAmenity: {AmenityName}, FieldAmenityId: {FieldAmenityId}", amenity.AmenityName, amenity.FieldAmenityId);
                            }
                            else
                            {
                                amenity = new FieldAmenity
                                {
                                    FieldId = fieldId,
                                    AmenityName = amenityDto.AmenityName,
                                    Description = amenityDto.Description,
                                    IconUrl = amenityDto.IconUrl,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _unitOfWork.Repository<FieldAmenity>().AddAsync(amenity);
                                _logger.LogInformation("Thêm mới FieldAmenity: {AmenityName}", amenity.AmenityName);
                            }
                        }

                        // Lưu Amenities
                        await _unitOfWork.SaveChangesAsync();

                        // Cập nhật Images
                        if (dto.Images != null && dto.Images.Any())
                        {
                            var existingImages = await (await _unitOfWork.Repository<FieldImage>()
                                .FindAsQueryableAsync(fi => fi.FieldId == fieldId && fi.DeletedAt == null))
                                .AsNoTracking()
                                .ToListAsync();

                            foreach (var image in existingImages)
                            {
                                if (!string.IsNullOrEmpty(image.PublicId))
                                {
                                    await _cloudinaryService.DeleteImageAsync(image.PublicId);
                                }
                                image.DeletedAt = DateTime.UtcNow;
                                _unitOfWork.Repository<FieldImage>().Update(image);
                                _logger.LogInformation("Đánh dấu xóa FieldImage, FieldImageId: {FieldImageId}", image.FieldImageId);
                            }

                            foreach (var image in dto.Images)
                            {
                                if (image == null || image.Length == 0)
                                {
                                    _logger.LogWarning("Hình ảnh không hợp lệ trong danh sách Images.");
                                    throw new InvalidOperationException("Một hoặc nhiều hình ảnh không hợp lệ.");
                                }
                                var uploadDto = new UploadFieldImageDto { Image = image, IsPrimary = false };
                                await UploadFieldImageAsync(fieldId, uploadDto, user);
                                _logger.LogInformation("Thêm mới FieldImage cho FieldId: {FieldId}", fieldId);
                            }
                        }

                        // Lưu tất cả thay đổi cuối cùng
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Hoàn tất cập nhật Field với ID: {FieldId}", fieldId);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Lỗi khi cập nhật sân trong giao dịch. FieldId: {FieldId}", fieldId);
                        throw new InvalidOperationException("Không thể cập nhật sân: " + ex.Message, ex);
                    }

                    // Lấy lại Field đã cập nhật
                    try
                    {
                        var updatedQuery = await _unitOfWork.Repository<Field>()
                            .FindAsQueryableAsync(f => f.FieldId == fieldId && f.DeletedAt == null);
                        var savedField = await updatedQuery
                            .Include(f => f.SubFields.Where(sf => sf.DeletedAt == null))
                                .ThenInclude(sf => sf.PricingRules.Where(pr => pr.DeletedAt == null))
                                .ThenInclude(pr => pr.TimeSlots.Where(ts => ts.DeletedAt == null))
                            .Include(f => f.FieldServices.Where(fs => fs.DeletedAt == null))
                            .Include(f => f.FieldAmenities.Where(fa => fa.DeletedAt == null))
                            .Include(f => f.FieldImages.Where(fi => fi.DeletedAt == null))
                            .FirstOrDefaultAsync();

                        if (savedField == null)
                        {
                            _logger.LogWarning("Không thể lấy lại sân đã cập nhật với ID: {FieldId}", fieldId);
                            throw new InvalidOperationException("Sân đã cập nhật không tìm thấy.");
                        }

                        var result = MapToFieldResponseDto(savedField, null, null);

                        // Cập nhật cache
                        var cacheKey = $"field_{fieldId}";
                        await _cache.SetRecordAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                        await _cache.RemoveAsync("fields_*");
                        _logger.LogInformation("Cập nhật cache cho FieldId: {FieldId}", fieldId);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi lấy lại sân đã cập nhật hoặc cập nhật cache cho FieldId: {FieldId}", fieldId);
                        throw new InvalidOperationException("Sân đã được cập nhật nhưng lỗi khi lấy dữ liệu: " + ex.Message, ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sân với ID: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Xóa mềm một sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin sân đã xóa.</returns>
        public async Task<DeleteFieldResponseDto> DeleteFieldAsync(int fieldId, ClaimsPrincipal user)
        {
            _logger.LogInformation("Xóa sân với ID: {FieldId}", fieldId);

            try
            {
                // Lấy Account từ ClaimsPrincipal
                var account = await _authService.GetCurrentUserAsync(user);
                var owner = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (owner == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin chủ sân.");
                }

                // Kiểm tra sân tồn tại và thuộc Owner
                var query = await _unitOfWork.Repository<Field>()
                    .FindAsQueryableAsync(f => f.FieldId == fieldId && f.OwnerId == owner.OwnerId && f.Status != "Deleted" && f.DeletedAt == null);
                var field = await query
                    .Include(f => f.SubFields).ThenInclude(sf => sf.Bookings)
                    .FirstOrDefaultAsync();

                if (field == null)
                {
                    _logger.LogWarning("Sân với FieldId {FieldId} không tồn tại hoặc không thuộc OwnerId {OwnerId}", fieldId, owner.OwnerId);
                    throw new KeyNotFoundException("Sân không tồn tại hoặc bạn không có quyền truy cập.");
                }

                // Kiểm tra đặt sân đang hoạt động
                var hasActiveBookings = field.SubFields.Any(sf => sf.Bookings.Any(b => (b.Status == "Confirmed" || b.Status == "Pending") && b.DeletedAt == null));
                if (hasActiveBookings)
                {
                    _logger.LogWarning("Sân ID {FieldId} có đặt sân đang hoạt động.", fieldId);
                    throw new InvalidOperationException("Không thể xóa sân vì còn đặt sân đang hoạt động.");
                }

                var strategy = _unitOfWork.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Xóa mềm sân
                        field.Status = "Deleted";
                        field.DeletedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<Field>().Update(field);
                        await _unitOfWork.SaveChangesAsync();

                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Commit transaction thành công cho xóa sân ID: {FieldId}", fieldId);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Lỗi khi xóa sân trong giao dịch. StackTrace: {StackTrace}", ex.StackTrace);
                        throw new InvalidOperationException("Không thể xóa sân: " + ex.Message, ex);
                    }
                });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sân ID: {FieldId}. StackTrace: {StackTrace}", fieldId, ex.StackTrace);
                throw;
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
        public async Task<PagedResult<BookingResponseDto>> GetFieldBookingsAsync(int fieldId, BookingFilterDto filter, ClaimsPrincipal user)
        {
            _logger.LogInformation("Lấy danh sách đặt sân cho sân ID: {FieldId}, bộ lọc: {@Filter}", fieldId, filter);

            try
            {
                // Lấy Account từ ClaimsPrincipal
                var account = await _authService.GetCurrentUserAsync(user);
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
                _logger.LogWarning("Token không hợp lệ.");
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var accountId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(accountId))
            {
                _logger.LogWarning("Không tìm thấy AccountId trong token. Claims: {Claims}", string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}")));
                throw new UnauthorizedAccessException("Token không chứa AccountId hợp lệ.");
            }
            _logger.LogInformation("AccountId từ token: {AccountId}, Role: {Role}", accountId, role);
            var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}