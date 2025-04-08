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
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception("Không tìm thấy người dùng");
            var userId = userEntity.UserId;

            var bookingDate = DateTime.Parse(createBookingDto.BookingDate);

            // Kiểm tra tất cả subfields và timeslots
            foreach (var subFieldDto in createBookingDto.SubFields)
            {
                var subField = await _unitOfWork.SubFields.GetByIdAsync(subFieldDto.SubFieldId);
                if (subField == null) throw new Exception($"Sân {subFieldDto.SubFieldId} không tồn tại");

                foreach (var slot in subFieldDto.TimeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.StartTime);
                    var endTime = TimeSpan.Parse(slot.EndTime);

                    var isAvailable = !await _unitOfWork.Bookings.AnyAsync(b =>
                        b.SubFieldId == subFieldDto.SubFieldId &&
                        b.BookingDate == bookingDate &&
                        b.StartTime < endTime && b.EndTime > startTime &&
                        b.Status != "Canceled");

                    if (!isAvailable) throw new Exception($"Khung giờ {slot.StartTime}-{slot.EndTime} tại sân {subFieldDto.SubFieldId} đã được đặt");
                }
            }

            // Tạo booking chính (Main Booking)
            var mainSubFieldDto = createBookingDto.SubFields.First();
            var mainSlot = mainSubFieldDto.TimeSlots.First();
            var mainBooking = new Booking
            {
                UserId = userId,
                SubFieldId = mainSubFieldDto.SubFieldId,
                BookingDate = bookingDate,
                StartTime = TimeSpan.Parse(mainSlot.StartTime),
                EndTime = TimeSpan.Parse(mainSlot.EndTime),
                Status = "Pending",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TimeSlots = new List<BookingTimeSlot>(),
                RelatedBookings = new List<Booking>()
            };

            decimal totalPrice = 0;

            // Thêm timeslots cho main booking
            var mainSubField = await _unitOfWork.SubFields.GetByIdAsync(mainSubFieldDto.SubFieldId);
            foreach (var slot in mainSubFieldDto.TimeSlots)
            {
                var startTime = TimeSpan.Parse(slot.StartTime);
                var endTime = TimeSpan.Parse(slot.EndTime);
                var duration = (endTime - startTime).TotalHours;
                var slotPrice = (decimal)duration * mainSubField.PricePerHour;

                mainBooking.TimeSlots.Add(new BookingTimeSlot
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = slotPrice
                });
                totalPrice += slotPrice;
            }

            // Tạo các booking phụ cho các subfields khác
            foreach (var subFieldDto in createBookingDto.SubFields.Skip(1))
            {
                var subField = await _unitOfWork.SubFields.GetByIdAsync(subFieldDto.SubFieldId);
                var relatedBooking = new Booking
                {
                    UserId = userId,
                    SubFieldId = subFieldDto.SubFieldId,
                    BookingDate = bookingDate,
                    StartTime = TimeSpan.Parse(subFieldDto.TimeSlots.First().StartTime),
                    EndTime = TimeSpan.Parse(subFieldDto.TimeSlots.First().EndTime),
                    Status = "Pending",
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    TimeSlots = new List<BookingTimeSlot>()
                };

                foreach (var slot in subFieldDto.TimeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.StartTime);
                    var endTime = TimeSpan.Parse(slot.EndTime);
                    var duration = (endTime - startTime).TotalHours;
                    var slotPrice = (decimal)duration * subField.PricePerHour;

                    relatedBooking.TimeSlots.Add(new BookingTimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        Price = slotPrice
                    });
                    totalPrice += slotPrice;
                }

                mainBooking.RelatedBookings.Add(relatedBooking);
            }

            // Thêm dịch vụ
            if (createBookingDto.ServiceIds != null && createBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    createBookingDto.ServiceIds.Contains(fs.FieldServiceId));

                foreach (var service in fieldServices)
                {
                    totalPrice += service.Price;
                    mainBooking.BookingServices.Add(new Models.BookingService
                    {
                        FieldServiceId = service.FieldServiceId,
                        Quantity = 1,
                        Price = service.Price
                    });
                }
            }

            mainBooking.TotalPrice = totalPrice;
            await _unitOfWork.Bookings.AddAsync(mainBooking);
            await _unitOfWork.SaveChangesAsync();

            // Gán MainBookingId cho các booking phụ
            foreach (var relatedBooking in mainBooking.RelatedBookings)
            {
                relatedBooking.MainBookingId = mainBooking.BookingId;
                _unitOfWork.Bookings.Update(relatedBooking);
            }
            await _unitOfWork.SaveChangesAsync();

            return await MapToBookingDto(mainBooking);
        }

        public async Task<PaginatedResponse<BookingDto>> GetBookingsAsync(
            ClaimsPrincipal user,
            string? status = null,
            int? fieldId = null,
            string? sort = null,
            int page = 1,
            int pageSize = 10)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isOwner = user.IsInRole("Owner");
            int? userId = null;
            int? ownerId = null;

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

            var query = _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                .Include(b => b.RelatedBookings)
                .Where(b => b.MainBookingId == null); // Chỉ lấy booking chính

            if (isOwner && ownerId.HasValue)
            {
                query = query.Where(b => b.SubField.Field.OwnerId == ownerId.Value);
                if (fieldId.HasValue)
                {
                    query = query.Where(b => b.SubField.FieldId == fieldId.Value);
                }
            }
            else if (userId.HasValue)
            {
                query = query.Where(b => b.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            query = ApplySorting(query, sort);

            var totalItems = await query.CountAsync();
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

            var booking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                .Include(b => b.RelatedBookings).ThenInclude(rb => rb.SubField)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return null;

            // Kiểm tra quyền truy cập
            if (isOwner)
            {
                var owner = await _unitOfWork.Owners.GetAll()
                    .FirstOrDefaultAsync(o => o.AccountId == accountId);
                if (owner == null || booking.SubField.Field.OwnerId != owner.OwnerId)
                    return null;
            }
            else
            {
                var userEntity = await _unitOfWork.Users.GetAll()
                    .FirstOrDefaultAsync(u => u.AccountId == accountId);
                if (userEntity == null || booking.UserId != userEntity.UserId)
                    return null;
            }

            // Nếu là booking phụ, lấy thông tin booking chính
            if (booking.MainBookingId.HasValue)
            {
                var mainBooking = await _unitOfWork.Bookings.GetAll()
                    .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                    .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService)
                    .FirstOrDefaultAsync(b => b.BookingId == booking.MainBookingId.Value);

                return await MapToBookingDto(mainBooking);
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> UpdateBookingAsync(ClaimsPrincipal user, int bookingId, UpdateBookingDto updateBookingDto)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
            var userId = userEntity.UserId;

            var mainBooking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.BookingServices)
                .Include(b => b.RelatedBookings)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (mainBooking == null) return null;
            if (mainBooking.Status != "Pending" || mainBooking.PaymentStatus != "Unpaid")
                throw new Exception("Không thể chỉnh sửa booking đã xác nhận hoặc đã thanh toán.");

            var bookingDate = DateTime.Parse(updateBookingDto.BookingDate);

            // Kiểm tra tính khả dụng của tất cả subfields và timeslots
            foreach (var subFieldDto in updateBookingDto.SubFields)
            {
                foreach (var slot in subFieldDto.TimeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.StartTime);
                    var endTime = TimeSpan.Parse(slot.EndTime);

                    var isAvailable = !await _unitOfWork.Bookings.AnyAsync(b =>
                        b.SubFieldId == subFieldDto.SubFieldId &&
                        b.BookingId != bookingId &&
                        b.BookingDate == bookingDate &&
                        b.StartTime < endTime && b.EndTime > startTime &&
                        b.Status != "Canceled");

                    if (!isAvailable)
                        throw new Exception($"Sân con ID {subFieldDto.SubFieldId} không còn trống trong khung giờ {slot.StartTime}-{slot.EndTime}");
                }
            }

            // Cập nhật main booking
            var mainSubFieldDto = updateBookingDto.SubFields.First();
            mainBooking.BookingDate = bookingDate;
            mainBooking.StartTime = TimeSpan.Parse(mainSubFieldDto.TimeSlots.First().StartTime);
            mainBooking.EndTime = TimeSpan.Parse(mainSubFieldDto.TimeSlots.First().EndTime);
            mainBooking.UpdatedAt = DateTime.UtcNow;
            mainBooking.TimeSlots.Clear();

            decimal totalPrice = 0;
            var mainSubField = await _unitOfWork.SubFields.GetByIdAsync(mainBooking.SubFieldId);
            foreach (var slot in mainSubFieldDto.TimeSlots)
            {
                var startTime = TimeSpan.Parse(slot.StartTime);
                var endTime = TimeSpan.Parse(slot.EndTime);
                var duration = (endTime - startTime).TotalHours;
                var slotPrice = (decimal)duration * mainSubField.PricePerHour;

                mainBooking.TimeSlots.Add(new BookingTimeSlot
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = slotPrice
                });
                totalPrice += slotPrice;
            }

            // Cập nhật các booking phụ
            var existingRelatedBookings = mainBooking.RelatedBookings.ToList();
            foreach (var relatedBooking in existingRelatedBookings)
            {
                _unitOfWork.Bookings.Delete(relatedBooking);
            }
            mainBooking.RelatedBookings.Clear();

            foreach (var subFieldDto in updateBookingDto.SubFields.Skip(1))
            {
                var subField = await _unitOfWork.SubFields.GetByIdAsync(subFieldDto.SubFieldId);
                var relatedBooking = new Booking
                {
                    UserId = userId,
                    SubFieldId = subFieldDto.SubFieldId,
                    MainBookingId = mainBooking.BookingId,
                    BookingDate = bookingDate,
                    StartTime = TimeSpan.Parse(subFieldDto.TimeSlots.First().StartTime),
                    EndTime = TimeSpan.Parse(subFieldDto.TimeSlots.First().EndTime),
                    Status = "Pending",
                    PaymentStatus = "Unpaid",
                    CreatedAt = mainBooking.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    TimeSlots = new List<BookingTimeSlot>()
                };

                foreach (var slot in subFieldDto.TimeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.StartTime);
                    var endTime = TimeSpan.Parse(slot.EndTime);
                    var duration = (endTime - startTime).TotalHours;
                    var slotPrice = (decimal)duration * subField.PricePerHour;

                    relatedBooking.TimeSlots.Add(new BookingTimeSlot
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        Price = slotPrice
                    });
                    totalPrice += slotPrice;
                }

                mainBooking.RelatedBookings.Add(relatedBooking);
            }

            // Cập nhật dịch vụ
            var existingServices = mainBooking.BookingServices.ToList();
            foreach (var service in existingServices)
            {
                await _unitOfWork.BookingServices.DeleteAsync(service);
            }
            mainBooking.BookingServices.Clear();

            if (updateBookingDto.ServiceIds != null && updateBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    updateBookingDto.ServiceIds.Contains(fs.FieldServiceId));

                foreach (var service in fieldServices)
                {
                    totalPrice += service.Price;
                    mainBooking.BookingServices.Add(new Models.BookingService
                    {
                        FieldServiceId = service.FieldServiceId,
                        Quantity = 1,
                        Price = service.Price
                    });
                }
            }

            mainBooking.TotalPrice = totalPrice;
            _unitOfWork.Bookings.Update(mainBooking);
            await _unitOfWork.SaveChangesAsync();

            return await MapToBookingDto(mainBooking);
        }

        public async Task CancelBookingAsync(ClaimsPrincipal user, int bookingId)
        {
            var accountId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userEntity = await _unitOfWork.Users.GetAll()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);
            if (userEntity == null) throw new Exception($"Không tìm thấy User liên kết với AccountId {accountId}.");
            var userId = userEntity.UserId;

            // Lấy booking chính và các booking liên quan
            var mainBooking = await _unitOfWork.Bookings.GetAll()
                .Include(b => b.RelatedBookings)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (mainBooking == null) throw new Exception("Không tìm thấy booking.");
            if (mainBooking.Status != "Pending" || mainBooking.PaymentStatus != "Unpaid")
                throw new Exception("Không thể hủy booking đã xác nhận hoặc đã thanh toán.");

            // Hủy tất cả booking liên quan
            mainBooking.Status = "Canceled";
            mainBooking.UpdatedAt = DateTime.UtcNow;

            foreach (var relatedBooking in mainBooking.RelatedBookings)
            {
                relatedBooking.Status = "Canceled";
                relatedBooking.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Bookings.Update(relatedBooking);
            }

            _unitOfWork.Bookings.Update(mainBooking);
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
            // Lấy thông tin tất cả subfields từ danh sách SubFieldIds
            var subFieldIds = createBookingDto.SubFields.Select(sf => sf.SubFieldId).ToList();
            var subFields = await _unitOfWork.SubFields.GetAll()
                .Include(sf => sf.Field)
                .Where(sf => subFieldIds.Contains(sf.SubFieldId))
                .ToListAsync();

            if (subFields.Count != subFieldIds.Count)
            {
                var missingIds = subFieldIds.Except(subFields.Select(sf => sf.SubFieldId));
                throw new Exception($"Một hoặc nhiều sân không tồn tại: {string.Join(", ", missingIds)}");
            }

            // Kiểm tra xem tất cả subfields có thuộc cùng một field không
            var fieldId = subFields.First().FieldId;
            if (subFields.Any(sf => sf.FieldId != fieldId))
            {
                throw new Exception("Tất cả subfields phải thuộc cùng một field");
            }

            decimal totalPrice = 0;
            double totalHours = 0;
            var subFieldPreviews = new List<SubFieldPreviewDto>();

            // Tính giá và tạo preview cho từng subfield
            foreach (var subFieldDto in createBookingDto.SubFields)
            {
                var subField = subFields.First(sf => sf.SubFieldId == subFieldDto.SubFieldId);
                var timeSlotsPreview = new List<TimeSlotPreviewDto>();
                double subFieldTotalHours = 0;
                decimal subFieldTotalPrice = 0;

                foreach (var slot in subFieldDto.TimeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.StartTime);
                    var endTime = TimeSpan.Parse(slot.EndTime);
                    var duration = (endTime - startTime).TotalHours;
                    var slotPrice = (decimal)duration * subField.PricePerHour;

                    timeSlotsPreview.Add(new TimeSlotPreviewDto
                    {
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Duration = duration,
                        SlotPrice = slotPrice
                    });

                    subFieldTotalHours += duration;
                    subFieldTotalPrice += slotPrice;
                }

                subFieldPreviews.Add(new SubFieldPreviewDto
                {
                    SubFieldId = subField.SubFieldId,
                    SubFieldName = subField.SubFieldName,
                    TimeSlots = timeSlotsPreview,
                    SubFieldTotalHours = subFieldTotalHours,
                    SubFieldTotalPrice = subFieldTotalPrice
                });

                totalHours += subFieldTotalHours;
                totalPrice += subFieldTotalPrice;
            }

            // Thêm dịch vụ
            var services = new List<BookingServiceDto>();
            if (createBookingDto.ServiceIds != null && createBookingDto.ServiceIds.Any())
            {
                var fieldServices = await _unitOfWork.FieldServices.FindAsync(fs =>
                    createBookingDto.ServiceIds.Contains(fs.FieldServiceId));

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
                FieldName = subFields.First().Field.FieldName,
                BookingDate = createBookingDto.BookingDate,
                SubFields = subFieldPreviews,
                TotalHours = totalHours,
                TotalPrice = totalPrice,
                Services = services
            };
        }

        private IQueryable<Booking> ApplySorting(IQueryable<Booking> query, string? sort)
        {
            if (string.IsNullOrEmpty(sort)) return query.OrderByDescending(b => b.BookingDate);

            var parts = sort.Split(':');
            var field = parts[0].ToLower();
            var direction = parts.Length > 1 && parts[1].ToLower() == "desc" ? "desc" : "asc";

            return field switch
            {
                "bookingdate" => direction == "desc"
                    ? query.OrderByDescending(b => b.BookingDate)
                    : query.OrderBy(b => b.BookingDate),
                "createdat" => direction == "desc"
                    ? query.OrderByDescending(b => b.CreatedAt)
                    : query.OrderBy(b => b.CreatedAt),
                "price" => direction == "desc"
                    ? query.OrderByDescending(b => b.TotalPrice)
                    : query.OrderBy(b => b.TotalPrice),
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

            // Map timeslots cho main booking
            var timeSlots = booking.TimeSlots.Select(ts => new TimeSlotPreviewDto
            {
                StartTime = ts.StartTime.ToString(@"hh\:mm"),
                EndTime = ts.EndTime.ToString(@"hh\:mm"),
                Duration = (ts.EndTime - ts.StartTime).TotalHours,
                SlotPrice = ts.Price
            }).ToList();

            // Map timeslots cho related bookings
            var relatedBookings = booking.RelatedBookings?.Select(rb => new RelatedBookingDto
            {
                BookingId = rb.BookingId,
                SubFieldId = rb.SubFieldId,
                SubFieldName = rb.SubField?.SubFieldName ?? "Unknown",
                Status = rb.Status,
                TimeSlots = rb.TimeSlots.Select(ts => new TimeSlotPreviewDto
                {
                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
                    Duration = (ts.EndTime - ts.StartTime).TotalHours,
                    SlotPrice = ts.Price
                }).ToList()
            }).ToList() ?? new List<RelatedBookingDto>();

            return new BookingDto
            {
                FieldId = subField?.FieldId ?? 0,
                BookingId = booking.BookingId,
                MainBookingId = booking.MainBookingId ?? 0,
                SubFieldId = booking.SubFieldId,
                FieldName = subField?.Field.FieldName ?? "Unknown",
                SubFieldName = subField?.SubFieldName ?? "Unknown",
                BookingDate = booking.BookingDate,
                TimeSlots = timeSlots, // Thay thế StartTime và EndTime
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                Services = services,
                RelatedBookings = relatedBookings,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }
    }
}