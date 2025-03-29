using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Booking;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IUnitOfWork unitOfWork, ILogger<BookingService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingDto> CreateBookingAsync(ClaimsPrincipal user, CreateBookingDto createBookingDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation("Creating booking for AccountId: {AccountId}, SubFieldId: {SubFieldId}", accountId, createBookingDto.SubFieldId);

            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
            var userId = userEntity.UserId;
            _logger.LogInformation("Mapped AccountId {AccountId} to UserId {UserId}", accountId, userId);

            var subField = await _unitOfWork.SubFields.GetByIdAsync(createBookingDto.SubFieldId);
            if (subField == null) throw new Exception("Sân nhỏ không tồn tại.");

            var bookingDate = DateTime.Parse(createBookingDto.BookingDate);
            var startTime = TimeSpan.Parse(createBookingDto.StartTime);
            var endTime = TimeSpan.Parse(createBookingDto.EndTime);
            var duration = (endTime - startTime).TotalHours;

            var isAvailable = !await _unitOfWork.Bookings.AnyAsync(b =>
                b.SubFieldId == createBookingDto.SubFieldId &&
                b.BookingDate == bookingDate &&
                b.StartTime < endTime && b.EndTime > startTime &&
                b.Status != "Canceled");
            if (!isAvailable) throw new Exception("Khung giờ đã được đặt.");

            decimal totalPrice = (decimal)duration * subField.PricePerHour;
            var booking = new Booking
            {
                UserId = userId,
                SubFieldId = createBookingDto.SubFieldId,
                BookingDate = bookingDate,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = totalPrice,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            var bookingServices = new List<Models.BookingService>();
            if (createBookingDto.ServiceIds != null && createBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    createBookingDto.ServiceIds.Contains(fs.FieldServiceId));
                _logger.LogInformation("Found {Count} field services for SubFieldId {SubFieldId}", fieldServices.Count(), createBookingDto.SubFieldId);
                foreach (var service in fieldServices)
                {
                    totalPrice += service.Price;
                    bookingServices.Add(new Models.BookingService
                    {
                        BookingId = booking.BookingId,
                        FieldServiceId = service.FieldServiceId,
                        Quantity = 1,
                        Price = service.Price
                    });
                }
                if (bookingServices.Any())
                {
                    await _unitOfWork.BookingServices.AddRangeAsync(bookingServices);
                    booking.TotalPrice = totalPrice;
                    _unitOfWork.Bookings.Update(booking);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return await MapToBookingDto(booking);
        }

        public async Task<PaginatedResponse<BookingDto>> GetBookingsAsync(
            ClaimsPrincipal user,
            string? status = null, // Thay đổi thành nullable
            int? fieldId = null,
            string? sort = null,
            int page = 1,
            int pageSize = 10)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isOwner = user.IsInRole("Owner");
            int? userId = null;
            int? ownerId = null;

            // Kiểm tra role và lấy ID tương ứng
            if (isOwner)
            {
                var owner = await _unitOfWork.Owners.GetAll()
                    .FirstOrDefaultAsync(o => o.AccountId == accountId);
                if (owner == null) throw new Exception($"Không tìm thấy Owner liên kết với AccountId {accountId}.");
                ownerId = owner.OwnerId;
            }
            else
            {
                var userEntity = await _unitOfWork.Users.GetAll()
                    .FirstOrDefaultAsync(u => u.AccountId == accountId);
                if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
                userId = userEntity.UserId;
            }

            var query = _unitOfWork.Bookings.GetAll();

            // Lọc booking dựa trên role
            if (isOwner && ownerId.HasValue)
            {
                query = query.Where(b => _unitOfWork.SubFields.GetAll()
                    .Include(sf => sf.Field)
                    .Any(sf => sf.SubFieldId == b.SubFieldId && sf.Field.OwnerId == ownerId.Value));
                if (fieldId.HasValue)
                {
                    query = query.Where(b => _unitOfWork.SubFields.GetAll()
                        .Any(sf => sf.SubFieldId == b.SubFieldId && sf.FieldId == fieldId.Value));
                }
            }
            else if (userId.HasValue)
            {
                query = query.Where(b => b.UserId == userId.Value);
            }
            else
            {
                throw new Exception("Không thể xác định người dùng hoặc chủ sân.");
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            query = query
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService);

            var totalItems = await query.CountAsync();
            query = ApplySorting(query, sort);

            var bookingEntities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var bookings = new List<BookingDto>();
            foreach (var booking in bookingEntities)
            {
                bookings.Add(await MapToBookingDto(booking));
            }

            return new PaginatedResponse<BookingDto>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = bookings
            };
        }

        public async Task<BookingDto> GetBookingByIdAsync(ClaimsPrincipal user, int bookingId)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isOwner = user.IsInRole("Owner");
            int? userId = null;
            int? ownerId = null;

            // Kiểm tra role và lấy ID tương ứng
            if (isOwner)
            {
                var owner = await _unitOfWork.Owners.GetAll()
                    .FirstOrDefaultAsync(o => o.AccountId == accountId);
                if (owner == null) throw new Exception($"Không tìm thấy Owner liên kết với AccountId {accountId}.");
                ownerId = owner.OwnerId;
            }
            else
            {
                var userEntity = await _unitOfWork.Users.GetAll()
                    .FirstOrDefaultAsync(u => u.AccountId == accountId);
                if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
                userId = userEntity.UserId;
            }

            var booking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return null;

            if (!isOwner && userId.HasValue && booking.UserId != userId.Value) return null;
            if (isOwner && ownerId.HasValue && booking.SubField.Field.OwnerId != ownerId.Value) return null;

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> UpdateBookingAsync(ClaimsPrincipal user, int bookingId, UpdateBookingDto updateBookingDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
            var userId = userEntity.UserId;

            var booking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);
            if (booking == null) return null;
            if (booking.Status != "Pending" || booking.PaymentStatus != "Unpaid")
                throw new Exception("Không thể chỉnh sửa booking đã xác nhận hoặc đã thanh toán.");

            var subField = await _unitOfWork.SubFields.GetByIdAsync(booking.SubFieldId);
            var bookingDate = DateTime.Parse(updateBookingDto.BookingDate);
            var startTime = TimeSpan.Parse(updateBookingDto.StartTime);
            var endTime = TimeSpan.Parse(updateBookingDto.EndTime);
            var duration = (endTime - startTime).TotalHours;

            var isAvailable = !await _unitOfWork.Bookings.AnyAsync(b =>
                b.SubFieldId == booking.SubFieldId &&
                b.BookingId != bookingId &&
                b.BookingDate == bookingDate &&
                b.StartTime < endTime && b.EndTime > startTime &&
                b.Status != "Canceled");
            if (!isAvailable) throw new Exception("Khung giờ đã được đặt.");

            booking.BookingDate = bookingDate;
            booking.StartTime = startTime;
            booking.EndTime = endTime;
            booking.UpdatedAt = DateTime.UtcNow;

            decimal totalPrice = (decimal)duration * subField.PricePerHour;
            var existingServices = booking.BookingServices.ToList();
            foreach (var service in existingServices)
            {
                await _unitOfWork.BookingServices.DeleteAsync(service);
            }

            var bookingServices = new List<Models.BookingService>();
            if (updateBookingDto.ServiceIds != null && updateBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    updateBookingDto.ServiceIds.Contains(fs.FieldServiceId) && fs.FieldId == subField.FieldId);
                foreach (var service in fieldServices)
                {
                    totalPrice += service.Price;
                    bookingServices.Add(new Models.BookingService
                    {
                        BookingId = booking.BookingId,
                        FieldServiceId = service.FieldServiceId,
                        Quantity = 1,
                        Price = service.Price
                    });
                }
            }
            booking.TotalPrice = totalPrice;

            if (bookingServices.Any())
            {
                await _unitOfWork.BookingServices.AddRangeAsync(bookingServices);
            }

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();
            return await MapToBookingDto(booking);
        }

        public async Task CancelBookingAsync(ClaimsPrincipal user, int bookingId)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
            var userId = userEntity.UserId;

            var booking = await _unitOfWork.Bookings.GetAll()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");
            if (booking.Status != "Pending" || booking.PaymentStatus != "Unpaid")
                throw new Exception("Không thể hủy booking đã xác nhận hoặc đã thanh toán.");

            booking.Status = "Canceled";
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BookingDto> UpdateBookingStatusAsync(ClaimsPrincipal user, int bookingId, string status)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var owner = await _unitOfWork.Owners.GetAll()
                .FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (owner == null) throw new Exception($"Không tìm thấy Owner liên kết với AccountId {accountId}.");
            var ownerId = owner.OwnerId;

            var booking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return null;
            if (booking.SubField.Field.OwnerId != ownerId) return null;

            booking.Status = status;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();
            return await MapToBookingDto(booking);
        }

        public async Task<List<BookingServiceDto>> GetBookingServicesAsync(ClaimsPrincipal user, int bookingId)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            var isOwner = user.IsInRole("Owner");
            var userId = userEntity?.UserId;

            var booking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return null;
            if (!isOwner && (userEntity == null || booking.UserId != userId)) return null;
            if (isOwner)
            {
                var owner = await _unitOfWork.Owners.GetAll()
                    .FirstOrDefaultAsync(o => o.AccountId == accountId);
                if (owner == null || booking.SubField.Field.OwnerId != owner.OwnerId) return null;
            }

            return booking.BookingServices.Select(bs => new BookingServiceDto
            {
                FieldServiceId = bs.FieldServiceId,
                ServiceName = bs.FieldService?.ServiceName ?? "Unknown",
                Price = bs.Price,
                Quantity = bs.Quantity
            }).ToList();
        }

        public async Task<BookingPreviewDto> PreviewBookingAsync(CreateBookingDto createBookingDto)
        {
            var subField = await _unitOfWork.SubFields.GetAll()
                .Include(sf => sf.Field)
                .FirstOrDefaultAsync(sf => sf.SubFieldId == createBookingDto.SubFieldId);
            if (subField == null) throw new Exception("Sân nhỏ không tồn tại.");

            var startTime = TimeSpan.Parse(createBookingDto.StartTime);
            var endTime = TimeSpan.Parse(createBookingDto.EndTime);
            var duration = (endTime - startTime).TotalHours;
            decimal totalPrice = (decimal)duration * subField.PricePerHour;

            var services = new List<BookingServiceDto>();
            if (createBookingDto.ServiceIds != null && createBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    createBookingDto.ServiceIds.Contains(fs.FieldServiceId));
                _logger.LogInformation("Found {Count} field services for SubFieldId {SubFieldId}", fieldServices.Count(), createBookingDto.SubFieldId);
                foreach (var service in fieldServices)
                {
                    totalPrice += service.Price;
                    services.Add(new BookingServiceDto
                    {
                        FieldServiceId = service.FieldServiceId,
                        ServiceName = service.ServiceName,
                        Price = service.Price,
                        Quantity = 1
                    });
                }
            }

            return new BookingPreviewDto
            {
                SubFieldId = subField.SubFieldId,
                FieldName = subField.Field.FieldName,
                SubFieldName = subField.SubFieldName,
                BookingDate = createBookingDto.BookingDate,
                StartTime = createBookingDto.StartTime,
                EndTime = createBookingDto.EndTime,
                TotalPrice = totalPrice,
                Services = services
            };
        }

        private IQueryable<Booking> ApplySorting(IQueryable<Booking> query, string? sort)
        {
            if (string.IsNullOrEmpty(sort)) return query.OrderByDescending(b => b.BookingDate);
            var parts = sort.Split(':');
            var field = parts[0];
            var direction = parts.Length > 1 && parts[1].ToLower() == "desc" ? "desc" : "asc";

            return field.ToLower() switch
            {
                "bookingdate" => direction == "desc" ? query.OrderByDescending(b => b.BookingDate) : query.OrderBy(b => b.BookingDate),
                "createdat" => direction == "desc" ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => query.OrderByDescending(b => b.BookingDate)
            };
        }

        private async Task<BookingDto> MapToBookingDto(Booking booking)
        {
            var subField = await _unitOfWork.SubFields.GetAll()
                .Include(sf => sf.Field)
                .FirstOrDefaultAsync(sf => sf.SubFieldId == booking.SubFieldId);

            var services = booking.BookingServices != null
                ? booking.BookingServices.Select(bs => new BookingServiceDto
                {
                    FieldServiceId = bs.FieldServiceId,
                    ServiceName = bs.FieldService?.ServiceName ?? "Unknown",
                    Price = bs.Price,
                    Quantity = bs.Quantity
                }).ToList()
                : new List<BookingServiceDto>();

            return new BookingDto
            {
                BookingId = booking.BookingId,
                SubFieldId = booking.SubFieldId,
                FieldName = subField?.Field.FieldName,
                SubFieldName = subField?.SubFieldName,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime.ToString(@"hh\:mm"),
                EndTime = booking.EndTime.ToString(@"hh\:mm"),
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                Services = services,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }
    }
}