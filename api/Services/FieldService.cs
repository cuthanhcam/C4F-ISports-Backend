using Microsoft.EntityFrameworkCore;
using api.Interfaces;
using api.Dtos.Field;
using api.Dtos;
using api.Models;
using System.Security.Claims;
using System.Linq;
using api.Data;

namespace api.Services
{
    public class FieldService : IFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FieldService> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public FieldService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<FieldService> logger,
            CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PaginatedResponse<FieldDto>> GetFieldsAsync(FieldFilterDto filter)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldPricings)
                .Include(f => f.FieldAmenities)
                .Include(f => f.Services)
                .Include(f => f.FieldDescriptions)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(f => f.Status == filter.Status);
            }

            if (filter.SportId.HasValue)
            {
                query = query.Where(f => f.SportId == filter.SportId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Location))
            {
                query = query.Where(f => f.Address.Contains(filter.Location));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(f => f.FieldPricings.Any(p => p.Price >= filter.MinPrice.Value));
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(f => f.FieldPricings.Any(p => p.Price <= filter.MaxPrice.Value));
            }

            // Áp dụng sắp xếp
            if (!string.IsNullOrEmpty(filter.Sort))
            {
                var sortParts = filter.Sort.Split(':');
                if (sortParts.Length == 2)
                {
                    var sortField = sortParts[0].ToLower();
                    var sortOrder = sortParts[1].ToLower();

                    query = sortField switch
                    {
                        "fieldname" => sortOrder == "asc" ? query.OrderBy(f => f.FieldName) : query.OrderByDescending(f => f.FieldName),
                        "location" => sortOrder == "asc" ? query.OrderBy(f => f.Address) : query.OrderByDescending(f => f.Address),
                        "rating" => sortOrder == "asc" ? query.OrderBy(f => f.Reviews.Average(r => r.Rating)) : query.OrderByDescending(f => f.Reviews.Average(r => r.Rating)),
                        "price" => sortOrder == "asc" ? query.OrderBy(f => f.FieldPricings.Min(p => p.Price)) : query.OrderByDescending(f => f.FieldPricings.Min(p => p.Price)),
                        _ => query.OrderBy(f => f.FieldName)
                    };
                }
            }
            else
            {
                query = query.OrderBy(f => f.FieldName);
            }

            // Tính toán phân trang
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResponse<FieldDto>
            {
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Items = items.Select(f => MapToFieldDto(f))
            };
        }

        public async Task<FieldDto> GetFieldByIdAsync(int id)
        {
            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldPricings)
                .Include(f => f.FieldAmenities)
                .Include(f => f.Services)
                .Include(f => f.FieldDescriptions)
                .FirstOrDefaultAsync(f => f.FieldId == id);

            return field != null ? MapToFieldDto(field) : null;
        }

        public async Task<PaginatedResponse<FieldDto>> SearchFieldsAsync(FieldSearchDto search)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldPricings)
                .Include(f => f.Bookings)
                .Include(f => f.FieldAmenities)
                .Include(f => f.Services)
                .Include(f => f.FieldDescriptions)
                .AsQueryable();

            // Bộ lọc SearchTerm (linh hoạt)
            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                var searchTermLower = search.SearchTerm.ToLower();
                query = query.Where(f =>
                    f.FieldName.ToLower().Contains(searchTermLower) ||
                    f.FieldDescriptions.Any(d => d.Description != null && d.Description.ToLower().Contains(searchTermLower)) ||
                    f.Address.ToLower().Contains(searchTermLower));
            }

            // Bộ lọc SportId (nghiêm ngặt)
            if (search.SportId.HasValue)
            {
                query = query.Where(f => f.SportId == search.SportId.Value);
            }

            // Bộ lọc MinPrice (linh hoạt)
            if (search.MinPrice.HasValue)
            {
                query = query.Where(f => !f.FieldPricings.Any() || f.FieldPricings.Any(p => p.Price >= search.MinPrice.Value));
            }

            // Bộ lọc MaxPrice (linh hoạt)
            if (search.MaxPrice.HasValue)
            {
                query = query.Where(f => !f.FieldPricings.Any() || f.FieldPricings.Any(p => p.Price <= search.MaxPrice.Value));
            }

            // Bộ lọc Location (tùy chọn, không bắt buộc khớp)
            if (search.Latitude.HasValue && search.Longitude.HasValue && search.Radius.HasValue)
            {
                query = query.Where(f =>
                    CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value) <= search.Radius.Value);
            }
            else if (!string.IsNullOrEmpty(search.Location))
            {
                var locationLower = search.Location.ToLower();
                // Chỉ áp dụng lọc Location nếu địa chỉ chứa chuỗi, nếu không thì không loại bỏ
                query = query.Where(f => f.Address.ToLower().Contains(locationLower) || !f.Address.ToLower().Contains(locationLower));
            }

            // Bộ lọc Time (linh hoạt)
            if (search.Time.HasValue)
            {
                var date = search.Time.Value.Date;
                var time = search.Time.Value.TimeOfDay;
                query = query.Where(f => f.FieldPricings.Any(p =>
                    p.DayOfWeek == (int)date.DayOfWeek &&
                    p.StartTime <= time && p.EndTime >= time &&
                    !f.Bookings.Any(b => b.BookingDate.Date == date &&
                                         b.StartTime <= p.EndTime && b.EndTime >= p.StartTime)));
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(search.Sort))
            {
                switch (search.Sort.ToLower())
                {
                    case "rating:asc":
                        query = query.OrderBy(f => f.Reviews.Any() ? f.Reviews.Average(r => r.Rating) : 0);
                        break;
                    case "rating:desc":
                        query = query.OrderByDescending(f => f.Reviews.Any() ? f.Reviews.Average(r => r.Rating) : 0);
                        break;
                    case "price:asc":
                        query = query.OrderBy(f => f.FieldPricings.Any() ? f.FieldPricings.Min(p => p.Price) : 0);
                        break;
                    case "price:desc":
                        query = query.OrderByDescending(f => f.FieldPricings.Any() ? f.FieldPricings.Max(p => p.Price) : decimal.MaxValue);
                        break;
                    case "distance:asc":
                        if (search.Latitude.HasValue && search.Longitude.HasValue)
                        {
                            query = query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value));
                        }
                        break;
                    case "distance:desc":
                        if (search.Latitude.HasValue && search.Longitude.HasValue)
                        {
                            query = query.OrderByDescending(f => CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value));
                        }
                        break;
                    default:
                        query = query.OrderBy(f => f.FieldName);
                        break;
                }
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(f => MapToFieldDto(f))
                .ToListAsync();

            return new PaginatedResponse<FieldDto>
            {
                TotalItems = totalItems,
                Page = search.Page,
                PageSize = search.PageSize,
                Items = items
            };
        }

        public async Task<FieldDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            // Lấy OwnerId từ bảng Owners dựa trên AccountId
            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (owner == null)
                throw new InvalidOperationException($"No Owner found for AccountId {accountId}");

            var field = new Field
            {
                FieldName = createFieldDto.FieldName,
                Address = createFieldDto.Address,
                Phone = createFieldDto.Phone,
                OpenHours = createFieldDto.OpenHours,
                Status = createFieldDto.Status,
                SportId = createFieldDto.SportId,
                Latitude = createFieldDto.Latitude,
                Longitude = createFieldDto.Longitude,
                OwnerId = owner.OwnerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Fields.AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            // Thêm mô tả
            if (!string.IsNullOrEmpty(createFieldDto.Description))
            {
                field.FieldDescriptions.Add(new FieldDescription
                {
                    FieldId = field.FieldId,
                    Description = createFieldDto.Description
                });
            }

            // Thêm các amenities (lấy tên từ database thay vì dùng ID)
            if (createFieldDto.AmenityIds != null && createFieldDto.AmenityIds.Any())
            {
                var amenities = await _unitOfWork.FieldAmenities
                    .GetAll()
                    .Where(a => createFieldDto.AmenityIds.Contains(a.FieldAmenityId))
                    .ToListAsync();

                foreach (var amenityId in createFieldDto.AmenityIds)
                {
                    var amenity = amenities.FirstOrDefault(a => a.FieldAmenityId == amenityId);
                    field.FieldAmenities.Add(new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        AmenityName = amenity?.AmenityName ?? $"Amenity_{amenityId}" // Fallback nếu không tìm thấy
                    });
                }
            }

            // Thêm các services
            if (createFieldDto.Services != null)
            {
                foreach (var service in createFieldDto.Services)
                {
                    field.Services.Add(new Service
                    {
                        FieldId = field.FieldId,
                        ServiceName = service.ServiceName,
                        Price = service.Price
                    });
                }
            }

            // Thêm các pricing
            if (createFieldDto.Pricing != null)
            {
                foreach (var price in createFieldDto.Pricing)
                {
                    field.FieldPricings.Add(new FieldPricing
                    {
                        FieldId = field.FieldId,
                        StartTime = price.StartTime,
                        EndTime = price.EndTime,
                        DayOfWeek = price.DayOfWeek,
                        Price = price.Price
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetFieldByIdAsync(field.FieldId);
        }

        public async Task<FieldDto> UpdateFieldAsync(ClaimsPrincipal user, int id, UpdateFieldDto updateFieldDto)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.FieldAmenities)
                .Include(f => f.Services)
                .Include(f => f.FieldPricings)
                .Include(f => f.FieldDescriptions)
                .FirstOrDefaultAsync(f => f.FieldId == id);

            if (field == null)
                return null;

            if (field.OwnerId != userId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            field.FieldName = updateFieldDto.FieldName;
            field.Address = updateFieldDto.Address;
            field.Phone = updateFieldDto.Phone;
            field.OpenHours = updateFieldDto.OpenHours;
            field.Status = updateFieldDto.Status;
            field.Latitude = updateFieldDto.Latitude;
            field.Longitude = updateFieldDto.Longitude;
            field.UpdatedAt = DateTime.UtcNow;

            // Cập nhật mô tả
            if (!string.IsNullOrEmpty(updateFieldDto.Description))
            {
                field.FieldDescriptions.Clear();
                field.FieldDescriptions.Add(new FieldDescription
                {
                    FieldId = field.FieldId,
                    Description = updateFieldDto.Description
                });
            }
            else
            {
                field.FieldDescriptions.Clear();
            }

            // Cập nhật amenities (lấy tên từ database)
            if (updateFieldDto.AmenityIds != null && updateFieldDto.AmenityIds.Any())
            {
                field.FieldAmenities.Clear();
                var amenities = await _unitOfWork.FieldAmenities
                    .GetAll()
                    .Where(a => updateFieldDto.AmenityIds.Contains(a.FieldAmenityId))
                    .ToListAsync();

                foreach (var amenityId in updateFieldDto.AmenityIds)
                {
                    var amenity = amenities.FirstOrDefault(a => a.FieldAmenityId == amenityId);
                    field.FieldAmenities.Add(new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        AmenityName = amenity?.AmenityName ?? $"Amenity_{amenityId}" // Fallback nếu không tìm thấy
                    });
                }
            }
            else
            {
                field.FieldAmenities.Clear();
            }

            // Cập nhật services
            if (updateFieldDto.Services != null)
            {
                field.Services.Clear();
                foreach (var service in updateFieldDto.Services)
                {
                    field.Services.Add(new Service
                    {
                        FieldId = field.FieldId,
                        ServiceName = service.ServiceName,
                        Price = service.Price,
                        ServiceId = service.ServiceId ?? 0 // Giữ ID nếu có, nếu không thì tạo mới
                    });
                }
            }
            else
            {
                field.Services.Clear();
            }

            // Cập nhật pricing
            if (updateFieldDto.Pricing != null)
            {
                field.FieldPricings.Clear();
                foreach (var price in updateFieldDto.Pricing)
                {
                    field.FieldPricings.Add(new FieldPricing
                    {
                        FieldId = field.FieldId,
                        StartTime = price.StartTime,
                        EndTime = price.EndTime,
                        DayOfWeek = price.DayOfWeek,
                        Price = price.Price,
                        FieldPricingId = price.PricingId ?? 0 // Giữ ID nếu có
                    });
                }
            }
            else
            {
                field.FieldPricings.Clear();
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetFieldByIdAsync(id);
        }

        public async Task DeleteFieldAsync(ClaimsPrincipal user, int id)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetByIdAsync(id);
            if (field == null)
                return;

            if (field.OwnerId != userId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            // Kiểm tra xem sân có đang được đặt không
            var hasBookings = await _unitOfWork.Bookings
                .GetAll()
                .AnyAsync(b => b.FieldId == id && b.Status != "Canceled" && b.Status != "Completed");
            if (hasBookings)
                throw new InvalidOperationException("Cannot delete field with active bookings");

            _unitOfWork.Fields.Delete(field);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<FieldReviewDto>> GetFieldReviewsAsync(
            int fieldId, int? rating, string sort, int page, int pageSize)
        {
            var query = _unitOfWork.Reviews.GetAll()
                .Include(r => r.User)
                .Where(r => r.FieldId == fieldId)
                .AsQueryable();

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                if (sortParts.Length == 2)
                {
                    var sortField = sortParts[0].ToLower();
                    var sortOrder = sortParts[1].ToLower();
                    query = sortField switch
                    {
                        "createdat" => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                        "rating" => sortOrder == "asc" ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                        _ => query.OrderByDescending(r => r.CreatedAt)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(r => r.CreatedAt);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<FieldReviewDto>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items.Select(r => new FieldReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserName = r.User?.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }

        public async Task<PaginatedResponse<FieldDto>> GetNearbyFieldsAsync(
            decimal latitude, decimal longitude, decimal radius, string sort, int page, int pageSize)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.FieldPricings)
                .Include(f => f.FieldAmenities)
                .Include(f => f.Services)
                .Include(f => f.FieldDescriptions)
                .Where(f => CalculateDistance(f.Latitude, f.Longitude, latitude, longitude) <= radius)
                .AsQueryable();

            // Áp dụng sắp xếp
            query = sort?.ToLower() switch
            {
                "distance" => query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, latitude, longitude)),
                "rating" => query.OrderByDescending(f => f.Reviews.Average(r => r.Rating)),
                "price" => query.OrderBy(f => f.FieldPricings.Min(p => p.Price)),
                _ => query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, latitude, longitude))
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<FieldDto>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items.Select(f => MapToFieldDto(f))
            };
        }

        public async Task<string> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, string imageBase64)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetByIdAsync(fieldId);
            if (field == null)
                throw new ArgumentException("Field not found");

            if (field.OwnerId != userId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            // Chuyển base64 thành stream để upload lên Cloudinary
            if (string.IsNullOrEmpty(imageBase64))
                throw new ArgumentException("Image data is empty or null");

            byte[] imageBytes = Convert.FromBase64String(imageBase64);
            using var stream = new MemoryStream(imageBytes);
            var file = new FormFile(stream, 0, imageBytes.Length, "fieldImage", "image.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" // Giả định là JPEG, có thể cần kiểm tra thêm
            };

            // Upload lên Cloudinary
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            if (string.IsNullOrEmpty(imageUrl))
                throw new Exception("Failed to upload image to Cloudinary");

            field.FieldImages.Add(new FieldImage
            {
                FieldId = fieldId,
                ImageUrl = imageUrl
            });

            await _unitOfWork.SaveChangesAsync();
            return imageUrl;
        }

        #region Unused Methods
        // public async Task<PaginatedResponse<FieldDto>> GetOwnerFieldsAsync(
        //     ClaimsPrincipal user, string status, string sort, int page, int pageSize)
        // {
        //     var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        //     if (userId == 0)
        //         throw new UnauthorizedAccessException("User not authenticated");

        //     var query = _unitOfWork.Fields.GetAll()
        //         .Include(f => f.Sport)
        //         .Include(f => f.FieldImages)
        //         .Include(f => f.FieldPricings)
        //         .Include(f => f.FieldAmenities)
        //         .Include(f => f.Services)
        //         .Include(f => f.FieldDescriptions)
        //         .Where(f => f.OwnerId == userId)
        //         .AsQueryable();

        //     if (!string.IsNullOrEmpty(status))
        //     {
        //         query = query.Where(f => f.Status == status);
        //     }

        //     // Áp dụng sắp xếp
        //     query = sort?.ToLower() switch
        //     {
        //         "name" => query.OrderBy(f => f.FieldName),
        //         "status" => query.OrderBy(f => f.Status),
        //         "created" => query.OrderByDescending(f => f.CreatedAt),
        //         _ => query.OrderByDescending(f => f.UpdatedAt)
        //     };

        //     var totalItems = await query.CountAsync();
        //     var items = await query
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();

        //     return new PaginatedResponse<FieldDto>
        //     {
        //         TotalItems = totalItems,
        //         Page = page,
        //         PageSize = pageSize,
        //         Items = items.Select(f => MapToFieldDto(f))
        //     };
        // }

        // public async Task ReportFieldAsync(int fieldId, FieldReportDto reportDto)
        // {
        //     // TODO: Implement field report functionality
        //     throw new NotImplementedException("Field report functionality is not implemented yet");
        // }

        // public async Task<PaginatedResponse<FieldDto>> GetSuggestedFieldsAsync(
        //     decimal? latitude, decimal? longitude, int page, int pageSize)
        // {
        //     var query = _unitOfWork.Fields.GetAll()
        //         .Include(f => f.Sport)
        //         .Include(f => f.FieldImages)
        //         .Include(f => f.FieldPricings)
        //         .Include(f => f.FieldAmenities)
        //         .Include(f => f.Services)
        //         .Include(f => f.FieldDescriptions)
        //         .Where(f => f.Status == "Active")
        //         .AsQueryable();

        //     if (latitude.HasValue && longitude.HasValue)
        //     {
        //         // Sắp xếp theo khoảng cách nếu có tọa độ
        //         query = query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, latitude.Value, longitude.Value));
        //     }
        //     else
        //     {
        //         // Sắp xếp theo đánh giá trung bình
        //         query = query.OrderByDescending(f => f.Reviews.Average(r => r.Rating));
        //     }

        //     var totalItems = await query.CountAsync();
        //     var items = await query
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();

        //     return new PaginatedResponse<FieldDto>
        //     {
        //         TotalItems = totalItems,
        //         Page = page,
        //         PageSize = pageSize,
        //         Items = items.Select(f => MapToFieldDto(f))
        //     };
        // }
        #endregion

        public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(int fieldId, DateTime date)
        {
            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.FieldPricings)
                .Include(f => f.Bookings)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId);

            if (field == null)
                return null;

            var dayOfWeek = (int)date.DayOfWeek;
            var pricing = field.FieldPricings
                .Where(p => p.DayOfWeek == dayOfWeek)
                .OrderBy(p => p.StartTime)
                .ToList();

            var slots = new List<TimeSlotDto>();
            foreach (var price in pricing)
            {
                var startTime = date.Date.Add(price.StartTime);
                var endTime = date.Date.Add(price.EndTime);

                // Kiểm tra xem khung giờ này đã được đặt chưa
                var isBooked = field.Bookings.Any(b =>
                    b.BookingDate.Date == date.Date &&
                    b.StartTime <= price.EndTime &&
                    b.EndTime >= price.StartTime);

                slots.Add(new TimeSlotDto
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = price.Price,
                    IsAvailable = !isBooked
                });
            }

            return slots;
        }

        private static FieldDto MapToFieldDto(Field field)
        {
            return new FieldDto
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                Address = field.Address,
                Phone = field.Phone,
                OpenHours = field.OpenHours,
                Description = field.FieldDescriptions.FirstOrDefault()?.Description,
                Status = field.Status,
                SportId = field.SportId,
                SportName = field.Sport?.SportName,
                Latitude = field.Latitude,
                Longitude = field.Longitude,
                Images = field.FieldImages.Select(i => i.ImageUrl).ToList(),
                Amenities = field.FieldAmenities.Select(a => new FieldAmenityDto
                {
                    AmenityId = a.FieldAmenityId,
                    AmenityName = a.AmenityName
                }).ToList(),
                Services = field.Services.Select(s => new FieldServiceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Price = s.Price
                }).ToList(),
                Pricing = field.FieldPricings.Select(p => new FieldPricingDto
                {
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    DayOfWeek = p.DayOfWeek,
                    Price = p.Price
                }).ToList()
            };
        }

        private decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371m; // Bán kính trái đất (km)
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = (decimal)Math.Sin((double)(dLat / 2)) * (decimal)Math.Sin((double)(dLat / 2)) +
                    (decimal)Math.Cos((double)ToRad(lat1)) * (decimal)Math.Cos((double)ToRad(lat2)) *
                    (decimal)Math.Sin((double)(dLon / 2)) * (decimal)Math.Sin((double)(dLon / 2));
            var c = 2 * (decimal)Math.Atan2(Math.Sqrt((double)a), Math.Sqrt(1 - (double)a));
            return R * c;
        }

        private decimal ToRad(decimal value)
        {
            return value * (decimal)Math.PI / 180;
        }
    }
}