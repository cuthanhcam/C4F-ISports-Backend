using Microsoft.EntityFrameworkCore;
using api.Interfaces;
using api.Dtos.Field;
using api.Dtos;
using api.Models;
using System.Security.Claims;
using System.Linq;
using api.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using api.Exceptions;

namespace api.Services
{
    public class FieldService : IFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FieldService> _logger;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IGeocodingService _geocodingService;

        public FieldService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<FieldService> logger,
            CloudinaryService cloudinaryService,
            IGeocodingService geocodingService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _geocodingService = geocodingService;
        }

        public async Task<PaginatedResponse<FieldDto>> GetFieldsAsync(FieldFilterDto filter)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices) // Đổi từ Services
                .Include(f => f.FieldDescriptions)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(f => f.Status == filter.Status);

            if (filter.SportId.HasValue)
                query = query.Where(f => f.SportId == filter.SportId.Value);

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(f => f.Address.Contains(filter.Location));

            if (filter.MinPrice.HasValue)
                query = query.Where(f => f.SubFields.Any(sf => sf.PricePerHour >= filter.MinPrice.Value));

            if (filter.MaxPrice.HasValue)
                query = query.Where(f => f.SubFields.Any(sf => sf.PricePerHour <= filter.MaxPrice.Value));

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
                        "price" => sortOrder == "asc" ? query.OrderBy(f => f.SubFields.Min(sf => sf.PricePerHour)) : query.OrderByDescending(f => f.SubFields.Min(sf => sf.PricePerHour)),
                        _ => query.OrderBy(f => f.FieldName)
                    };
                }
            }
            else
            {
                query = query.OrderBy(f => f.FieldName);
            }

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
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldDescriptions)
                .FirstOrDefaultAsync(f => f.FieldId == id);

            return field != null ? MapToFieldDto(field) : null;
        }

        public async Task<FieldDto> CreateFieldAsync(ClaimsPrincipal user, CreateFieldDto createFieldDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (owner == null)
                throw new ResourceNotFoundException($"No Owner found for AccountId {accountId}");

            var normalizedAddress = NormalizeAddress(createFieldDto.Address);
            decimal latitude = 0m; // Tọa độ mặc định nếu thất bại
            decimal longitude = 0m;
            try
            {
                (latitude, longitude) = await _geocodingService.GetCoordinatesFromAddressAsync(normalizedAddress);
                if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180)
                    throw new AppException("Tọa độ không hợp lệ từ dịch vụ địa chỉ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding failed for address: {Address}. Using default coordinates (0,0)", normalizedAddress);
                latitude = 0m; // Fallback tọa độ
                longitude = 0m;
                // Không ném exception để tiếp tục tạo field
            }

            var field = new Field
            {
                FieldName = createFieldDto.FieldName,
                Address = createFieldDto.Address,
                Phone = createFieldDto.Phone,
                OpenHours = createFieldDto.OpenHours,
                Status = createFieldDto.Status,
                SportId = createFieldDto.SportId,
                Latitude = latitude,
                Longitude = longitude,
                OwnerId = owner.OwnerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Fields.AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            // Logic thêm description, amenities, services, subfields giữ nguyên
            if (!string.IsNullOrEmpty(createFieldDto.Description))
            {
                field.FieldDescriptions.Add(new FieldDescription
                {
                    FieldId = field.FieldId,
                    Description = createFieldDto.Description
                });
            }

            if (createFieldDto.Amenities != null && createFieldDto.Amenities.Any())
            {
                foreach (var amenity in createFieldDto.Amenities)
                {
                    field.FieldAmenities.Add(new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        AmenityName = amenity.Name,
                        Description = amenity.Description ?? string.Empty
                    });
                }
            }

            if (createFieldDto.Services != null && createFieldDto.Services.Any())
            {
                foreach (var service in createFieldDto.Services)
                {
                    field.FieldServices.Add(new Models.FieldService
                    {
                        FieldId = field.FieldId,
                        ServiceName = service.ServiceName,
                        Price = service.Price,
                        Description = service.Description ?? string.Empty
                    });
                }
            }

            if (createFieldDto.SubFields != null && createFieldDto.SubFields.Any())
            {
                foreach (var subFieldDto in createFieldDto.SubFields)
                {
                    field.SubFields.Add(new SubField
                    {
                        FieldId = field.FieldId,
                        SubFieldName = subFieldDto.SubFieldName,
                        Size = subFieldDto.Size,
                        PricePerHour = subFieldDto.PricePerHour,
                        Status = "Active"
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetFieldByIdAsync(field.FieldId);
        }

        public async Task<FieldDto> UpdateFieldAsync(ClaimsPrincipal user, int id, UpdateFieldDto updateFieldDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.SubFields)
                .Include(f => f.FieldDescriptions)
                .FirstOrDefaultAsync(f => f.FieldId == id);

            if (field == null)
                throw new ResourceNotFoundException($"Field with ID {id} not found");

            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (field.OwnerId != owner?.OwnerId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            var normalizedAddress = NormalizeAddress(updateFieldDto.Address);
            decimal latitude = 0m; // Tọa độ mặc định nếu thất bại
            decimal longitude = 0m;
            try
            {
                (latitude, longitude) = await _geocodingService.GetCoordinatesFromAddressAsync(normalizedAddress);
                if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180)
                    throw new AppException("Tọa độ không hợp lệ từ dịch vụ địa chỉ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding failed for address: {Address}. Using default coordinates (0,0)", normalizedAddress);
                latitude = 0m; // Fallback tọa độ
                longitude = 0m;
                // Không ném exception để tiếp tục tạo field
            }

            field.FieldName = updateFieldDto.FieldName;
            field.Address = updateFieldDto.Address;
            field.Phone = updateFieldDto.Phone;
            field.OpenHours = updateFieldDto.OpenHours;
            field.Status = updateFieldDto.Status;
            field.Latitude = latitude;
            field.Longitude = longitude;
            field.UpdatedAt = DateTime.UtcNow;

            // Logic cập nhật description, amenities, services, subfields giữ nguyên
            field.FieldDescriptions.Clear();
            if (!string.IsNullOrEmpty(updateFieldDto.Description))
            {
                field.FieldDescriptions.Add(new FieldDescription
                {
                    FieldId = field.FieldId,
                    Description = updateFieldDto.Description
                });
            }

            field.FieldAmenities.Clear();
            if (updateFieldDto.Amenities != null && updateFieldDto.Amenities.Any())
            {
                foreach (var amenity in updateFieldDto.Amenities)
                {
                    field.FieldAmenities.Add(new FieldAmenity
                    {
                        FieldId = field.FieldId,
                        AmenityName = amenity.Name,
                        Description = amenity.Description ?? string.Empty
                    });
                }
            }

            field.FieldServices.Clear();
            if (updateFieldDto.Services != null && updateFieldDto.Services.Any())
            {
                foreach (var service in updateFieldDto.Services)
                {
                    field.FieldServices.Add(new Models.FieldService
                    {
                        FieldId = field.FieldId,
                        ServiceName = service.ServiceName,
                        Price = service.Price,
                        Description = service.Description ?? string.Empty
                    });
                }
            }

            field.SubFields.Clear();
            if (updateFieldDto.SubFields != null && updateFieldDto.SubFields.Any())
            {
                foreach (var subFieldDto in updateFieldDto.SubFields)
                {
                    field.SubFields.Add(new SubField
                    {
                        FieldId = field.FieldId,
                        SubFieldName = subFieldDto.SubFieldName,
                        Size = subFieldDto.Size,
                        PricePerHour = subFieldDto.PricePerHour,
                        Status = subFieldDto.Status ?? "Active"
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetFieldByIdAsync(id);
        }

        private string NormalizeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogError("Address is null or empty");
                throw new ArgumentException("Địa chỉ không được để trống");
            }

            // Loại bỏ khoảng trắng thừa
            address = address.Trim();

            // Chuẩn hóa dấu phẩy
            address = address.Replace(" ,", ",").Replace(", ", ",");

            // Loại bỏ "Việt Nam" hoặc "Vietnam" ở cuối nếu có
            address = address.TrimEnd(new[] { ',', ' ' });
            if (address.EndsWith(",Việt Nam", StringComparison.OrdinalIgnoreCase) ||
                address.EndsWith(",Vietnam", StringComparison.OrdinalIgnoreCase))
            {
                address = address.Substring(0, address.LastIndexOf(','));
            }

            // Thêm quốc gia nếu chưa có
            if (!address.EndsWith(", Việt Nam", StringComparison.OrdinalIgnoreCase) &&
                !address.EndsWith(", Vietnam", StringComparison.OrdinalIgnoreCase))
            {
                address = $"{address}, Việt Nam";
            }

            _logger.LogDebug("Normalized address from '{Original}' to '{Normalized}'", address, address);
            return address;
        }

        public async Task DeleteFieldAsync(ClaimsPrincipal user, int id)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.SubFields)
                    .ThenInclude(sf => sf.Bookings)
                .FirstOrDefaultAsync(f => f.FieldId == id);
            if (field == null)
                return;

            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (field.OwnerId != owner.OwnerId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            var hasBookings = field.SubFields.Any(sf => sf.Bookings.Any(b => b.Status != "Canceled" && b.Status != "Completed"));
            if (hasBookings)
                throw new InvalidOperationException("Cannot delete field with active bookings");

            _unitOfWork.Fields.Delete(field);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> UploadFieldImageAsync(ClaimsPrincipal user, int fieldId, string imageBase64)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var field = await _unitOfWork.Fields.GetByIdAsync(fieldId);
            if (field == null)
                throw new ArgumentException("Field not found");

            if (field.OwnerId != accountId)
                throw new UnauthorizedAccessException("User is not the owner of this field");

            if (string.IsNullOrEmpty(imageBase64))
                throw new ArgumentException("Image data is empty or null");

            byte[] imageBytes = Convert.FromBase64String(imageBase64);
            using var stream = new MemoryStream(imageBytes);
            var file = new FormFile(stream, 0, imageBytes.Length, "fieldImage", "image.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

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

        public async Task<PaginatedResponse<FieldDto>> GetOwnerFieldsAsync(
            ClaimsPrincipal user, string status, string sort, int page, int pageSize)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
                throw new UnauthorizedAccessException("User not authenticated");

            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (owner == null)
                throw new InvalidOperationException($"No Owner found for AccountId {accountId}");

            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldDescriptions)
                .Where(f => f.OwnerId == owner.OwnerId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);

            query = sort?.ToLower() switch
            {
                "name" => query.OrderBy(f => f.FieldName),
                "status" => query.OrderBy(f => f.Status),
                "created" => query.OrderByDescending(f => f.CreatedAt),
                _ => query.OrderByDescending(f => f.UpdatedAt)
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

        public async Task<FieldAvailabilityDto> GetFieldAvailabilityAsync(int fieldId, DateTime date)
        {
            var field = await _unitOfWork.Fields.GetAll()
                .Include(f => f.FieldServices)
                .Include(f => f.SubFields)
                    .ThenInclude(sf => sf.Bookings)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId);

            if (field == null)
                return null;

            var openHours = field.OpenHours.Split('-');
            var startTime = TimeSpan.Parse(openHours[0]);
            var endTime = TimeSpan.Parse(openHours[1]);

            var subFields = new List<SubFieldAvailabilityDto>();
            foreach (var subField in field.SubFields)
            {
                var slots = new List<TimeSlotDto>();
                var currentTime = startTime;

                while (currentTime < endTime)
                {
                    var slotEndTime = currentTime.Add(TimeSpan.FromHours(1));
                    var isBooked = subField.Bookings.Any(b =>
                        b.BookingDate.Date == date.Date &&
                        b.StartTime <= currentTime &&
                        b.EndTime > currentTime);

                    slots.Add(new TimeSlotDto
                    {
                        StartTime = currentTime.ToString(@"hh\:mm"), // Chỉ lấy giờ
                        EndTime = slotEndTime.ToString(@"hh\:mm"),   // Chỉ lấy giờ
                        Status = isBooked ? "Booked" : "Available"
                    });

                    currentTime = slotEndTime;
                }

                subFields.Add(new SubFieldAvailabilityDto
                {
                    SubFieldId = subField.SubFieldId,
                    Name = subField.SubFieldName,
                    PricePerHour = subField.PricePerHour,
                    Slots = slots
                });
            }

            return new FieldAvailabilityDto
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                Date = date.ToString("yyyy-MM-dd"),
                OpenHours = field.OpenHours,
                Services = field.FieldServices.Select(s => new FieldServiceAvailabilityDto
                {
                    Name = s.ServiceName,
                    Price = s.Price
                }).ToList(),
                SubFields = subFields
            };
        }

        private static FieldDto MapToFieldDto(Field field)
        {
            return new FieldDto
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                SportId = field.SportId,
                SportName = field.Sport?.SportName,
                Address = field.Address,
                Phone = field.Phone,
                OpenHours = field.OpenHours,
                Latitude = field.Latitude,
                Longitude = field.Longitude,
                Status = field.Status,
                Images = field.FieldImages.Select(i => i.ImageUrl).ToList(),
                Amenities = field.FieldAmenities.Select(a => new FieldAmenityDto
                {
                    AmenityName = a.AmenityName,
                    Description = a.Description
                }).ToList(),
                Services = field.FieldServices.Select(s => new FieldServiceDto
                {
                    Name = s.ServiceName,
                    Price = s.Price,
                    Description = s.Description
                }).ToList(),
                SubFields = field.SubFields.Select(sf => new SubFieldDto
                {
                    SubFieldId = sf.SubFieldId,
                    Name = sf.SubFieldName,
                    Size = sf.Size,
                    PricePerHour = sf.PricePerHour,
                    Status = sf.Status
                }).ToList()
            };
        }

        public async Task<PaginatedResponse<FieldReviewDto>> GetFieldReviewsAsync(
            int fieldId, int? rating, string sort, int page, int pageSize)
        {
            var query = _unitOfWork.Reviews.GetAll()
                .Include(r => r.User)
                .Where(r => r.FieldId == fieldId)
                .AsQueryable();

            if (rating.HasValue)
                query = query.Where(r => r.Rating == rating.Value);

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
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldDescriptions)
                .Where(f => CalculateDistance(f.Latitude, f.Longitude, latitude, longitude) <= radius)
                .AsQueryable();

            query = sort?.ToLower() switch
            {
                "distance" => query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, latitude, longitude)),
                "rating" => query.OrderByDescending(f => f.Reviews.Average(r => r.Rating)),
                "price" => query.OrderBy(f => f.SubFields.Min(sf => sf.PricePerHour)),
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

        public async Task<PaginatedResponse<FieldDto>> SearchFieldsAsync(FieldSearchDto search)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldDescriptions)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                var searchTermLower = search.SearchTerm.ToLower();
                query = query.Where(f =>
                    f.FieldName.ToLower().Contains(searchTermLower) ||
                    f.FieldDescriptions.Any(d => d.Description != null && d.Description.ToLower().Contains(searchTermLower)) ||
                    f.Address.ToLower().Contains(searchTermLower));
            }

            if (search.SportId.HasValue)
                query = query.Where(f => f.SportId == search.SportId.Value);

            if (search.MinPrice.HasValue)
                query = query.Where(f => f.SubFields.Any(sf => sf.PricePerHour >= search.MinPrice.Value));

            if (search.MaxPrice.HasValue)
                query = query.Where(f => f.SubFields.Any(sf => sf.PricePerHour <= search.MaxPrice.Value));

            if (search.Latitude.HasValue && search.Longitude.HasValue && search.Radius.HasValue)
            {
                query = query.Where(f =>
                    CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value) <= search.Radius.Value);
            }
            else if (!string.IsNullOrEmpty(search.Location))
            {
                var locationLower = search.Location.ToLower();
                query = query.Where(f => f.Address.ToLower().Contains(locationLower));
            }

            if (search.Time.HasValue)
            {
                var date = search.Time.Value.Date;
                var time = search.Time.Value.TimeOfDay;
                query = query.Where(f => f.SubFields.Any(sf =>
                    !sf.Bookings.Any(b => b.BookingDate.Date == date &&
                                         b.StartTime <= time && b.EndTime >= time)));
            }

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
                        query = query.OrderBy(f => f.SubFields.Any() ? f.SubFields.Min(sf => sf.PricePerHour) : 0);
                        break;
                    case "price:desc":
                        query = query.OrderByDescending(f => f.SubFields.Any() ? f.SubFields.Max(sf => sf.PricePerHour) : decimal.MaxValue);
                        break;
                    case "distance:asc":
                        if (search.Latitude.HasValue && search.Longitude.HasValue)
                            query = query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value));
                        break;
                    case "distance:desc":
                        if (search.Latitude.HasValue && search.Longitude.HasValue)
                            query = query.OrderByDescending(f => CalculateDistance(f.Latitude, f.Longitude, search.Latitude.Value, search.Longitude.Value));
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

        public async Task ReportFieldAsync(int fieldId, FieldReportDto reportDto)
        {
            var field = await _unitOfWork.Fields.GetByIdAsync(fieldId);
            if (field == null)
                throw new ArgumentException("Field not found");

            _logger.LogInformation("Field {FieldId} reported: {Reason}", fieldId, reportDto.Reason);
            // TODO: Thêm logic lưu báo cáo vào DB nếu cần
        }

        public async Task<PaginatedResponse<FieldDto>> GetSuggestedFieldsAsync(
            decimal? latitude, decimal? longitude, int page, int pageSize)
        {
            var query = _unitOfWork.Fields.GetAll()
                .Include(f => f.Sport)
                .Include(f => f.FieldImages)
                .Include(f => f.SubFields)
                .Include(f => f.FieldAmenities)
                .Include(f => f.FieldServices)
                .Include(f => f.FieldDescriptions)
                .Where(f => f.Status == "Active")
                .AsQueryable();

            if (latitude.HasValue && longitude.HasValue)
            {
                query = query.OrderBy(f => CalculateDistance(f.Latitude, f.Longitude, latitude.Value, longitude.Value));
            }
            else
            {
                query = query.OrderByDescending(f => f.Reviews.Average(r => r.Rating));
            }

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