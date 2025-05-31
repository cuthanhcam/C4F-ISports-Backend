using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using api.Dtos.Booking;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using api.Dtos.Payment;

namespace api.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILogger<BookingService> _logger;
        private readonly IPaymentService _paymentService; // Giả định có service xử lý thanh toán

        public BookingService(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            ILogger<BookingService> logger,
            IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
            _paymentService = paymentService;
        }

        public async Task<PreviewBookingResponseDto> PreviewBookingAsync(ClaimsPrincipal user, PreviewBookingRequestDto request)
        {
            _logger.LogInformation("Xem trước đặt sân cho SubFieldId: {SubFieldId}", request.SubFieldId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xem trước đặt sân.");

            var subFieldQuery = _unitOfWork.Repository<SubField>()
                .FindQueryable(sf => sf.SubFieldId == request.SubFieldId && sf.Status == "Active" && sf.DeletedAt == null)
                .Include(sf => sf.Field)
                .Include(sf => sf.PricingSchedules);

            var subField = await subFieldQuery.FirstOrDefaultAsync();
            if (subField == null)
                throw new ArgumentException("Sân con không tồn tại hoặc không hoạt động.");

            if (!TimeSpan.TryParse(request.StartTime, out var startTime) || !TimeSpan.TryParse(request.EndTime, out var endTime))
                throw new ArgumentException("Thời gian không hợp lệ.");

            if (startTime >= endTime || endTime <= startTime)
                throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

            // Kiểm tra khung giờ hoạt động
            if (startTime < subField.OpenTime || endTime > subField.CloseTime)
                throw new ArgumentException("Thời gian đặt sân ngoài khung giờ hoạt động.");

            // Kiểm tra tính khả dụng
            var isAvailable = await CheckTimeSlotAvailabilityAsync(request.SubFieldId, request.BookingDate, startTime, endTime);
            if (!isAvailable)
                throw new ArgumentException("Khung giờ đã được đặt.");

            // Tính giá cơ bản
            decimal basePrice = await CalculateBasePriceAsync(subField, request.BookingDate, startTime, endTime);

            // Tính giá dịch vụ
            decimal servicePrice = 0;
            if (request.Services != null && request.Services.Any())
            {
                foreach (var service in request.Services)
                {
                    var fieldService = await _unitOfWork.Repository<Models.FieldService>()
                        .FindSingleAsync(fs => fs.FieldServiceId == service.FieldServiceId && fs.FieldId == subField.FieldId && fs.IsActive && fs.DeletedAt == null);
                    if (fieldService == null)
                        throw new ArgumentException($"Dịch vụ {service.FieldServiceId} không tồn tại hoặc không khả dụng.");
                    servicePrice += fieldService.Price * service.Quantity;
                }
            }

            // Áp dụng khuyến mãi
            decimal discount = 0;
            if (!string.IsNullOrEmpty(request.PromotionCode))
            {
                var promotion = await _unitOfWork.Repository<Promotion>()
                    .FindSingleAsync(p => p.Code == request.PromotionCode && p.IsActive && p.DeletedAt == null &&
                                         p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow &&
                                         (p.FieldId == null || p.FieldId == subField.FieldId));
                if (promotion != null && basePrice >= (promotion.MinBookingValue ?? 0))
                {
                    discount = promotion.DiscountType == "Percentage"
                        ? basePrice * (promotion.DiscountValue ?? 0) / 100
                        : promotion.DiscountValue ?? 0;
                    if (promotion.MaxDiscountAmount.HasValue && discount > promotion.MaxDiscountAmount.Value)
                        discount = promotion.MaxDiscountAmount.Value;
                }
            }

            var totalPrice = basePrice + servicePrice - discount;

            return new PreviewBookingResponseDto
            {
                SubFieldId = request.SubFieldId,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                BasePrice = basePrice,
                ServicePrice = servicePrice,
                Discount = discount,
                TotalPrice = totalPrice,
                IsAvailable = isAvailable
            };
        }

        public async Task<SimpleBookingResponseDto> CreateSimpleBookingAsync(ClaimsPrincipal user, SimpleBookingRequestDto request)
        {
            _logger.LogInformation("Tạo đặt sân đơn giản cho SubFieldId: {SubFieldId}", request.SubFieldId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể tạo đặt sân.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            var subFieldQuery = _unitOfWork.Repository<SubField>()
                .FindQueryable(sf => sf.SubFieldId == request.SubFieldId && sf.Status == "Active" && sf.DeletedAt == null)
                .Include(sf => sf.Field)
                .Include(sf => sf.PricingSchedules);

            var subField = await subFieldQuery.FirstOrDefaultAsync();
            if (subField == null)
                throw new ArgumentException("Sân con không tồn tại hoặc không hoạt động.");

            if (!TimeSpan.TryParse(request.StartTime, out var startTime) || !TimeSpan.TryParse(request.EndTime, out var endTime))
                throw new ArgumentException("Thời gian không hợp lệ.");

            if (startTime >= endTime)
                throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

            if (startTime < subField.OpenTime || endTime > subField.CloseTime)
                throw new ArgumentException("Thời gian đặt sân ngoài khung giờ hoạt động.");

            var isAvailable = await CheckTimeSlotAvailabilityAsync(request.SubFieldId, request.BookingDate, startTime, endTime);
            if (!isAvailable)
                throw new ArgumentException("Khung giờ đã được đặt.");

            decimal totalPrice = await CalculateBasePriceAsync(subField, request.BookingDate, startTime, endTime);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = new api.Models.Booking
                {
                    UserId = userEntity.UserId,
                    SubFieldId = request.SubFieldId,
                    BookingDate = request.BookingDate,
                    TotalPrice = totalPrice,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<api.Models.Booking>().AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                var timeSlot = new BookingTimeSlot
                {
                    BookingId = booking.BookingId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = totalPrice
                };
                await _unitOfWork.Repository<BookingTimeSlot>().AddAsync(timeSlot);
                booking.TimeSlots.Add(timeSlot);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SimpleBookingResponseDto
                {
                    BookingId = booking.BookingId,
                    SubFieldId = booking.SubFieldId,
                    BookingDate = booking.BookingDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status,
                    PaymentStatus = booking.PaymentStatus,
                    CreatedAt = booking.CreatedAt,
                    Message = "Đặt sân thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreateBookingResponseDto> CreateBookingAsync(ClaimsPrincipal user, CreateBookingRequestDto request)
        {
            _logger.LogInformation("Tạo nhiều đặt sân");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể tạo đặt sân.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            if (request.Bookings == null || !request.Bookings.Any())
                throw new ArgumentException("Danh sách đặt sân không được trống.");

            if (request.Bookings.Count > 5)
                throw new ArgumentException("Tối đa 5 đặt sân mỗi yêu cầu.");

            decimal totalPrice = 0;
            decimal discount = 0;
            var response = new CreateBookingResponseDto { Bookings = new List<BookingItemResponseDto>() };

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var mainBooking = new api.Models.Booking
                {
                    UserId = userEntity.UserId,
                    SubFieldId = request.Bookings.First().SubFieldId, // Main booking cần SubFieldId
                    BookingDate = request.Bookings.First().BookingDate,
                    TotalPrice = 0, // Sẽ cập nhật sau
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<api.Models.Booking>().AddAsync(mainBooking);
                await _unitOfWork.SaveChangesAsync();
                response.MainBookingId = mainBooking.BookingId;

                foreach (var bookingItem in request.Bookings)
                {
                    var subFieldQuery = _unitOfWork.Repository<SubField>()
                        .FindQueryable(sf => sf.SubFieldId == bookingItem.SubFieldId && sf.Status == "Active" && sf.DeletedAt == null)
                        .Include(sf => sf.Field)
                        .Include(sf => sf.PricingSchedules);

                    var subField = await subFieldQuery.FirstOrDefaultAsync();
                    if (subField == null)
                        throw new ArgumentException($"Sân con {bookingItem.SubFieldId} không tồn tại hoặc không hoạt động.");

                    if (bookingItem.TimeSlots == null || !bookingItem.TimeSlots.Any())
                        throw new ArgumentException("Danh sách time slot không được trống.");

                    if (bookingItem.TimeSlots.Count > 10)
                        throw new ArgumentException("Tối đa 10 time slot mỗi đặt sân.");

                    if (bookingItem.Services != null && bookingItem.Services.Count > 20)
                        throw new ArgumentException("Tối đa 20 dịch vụ mỗi đặt sân.");

                    decimal subTotal = 0;
                    var timeSlots = new List<BookingTimeSlot>();
                    foreach (var timeSlot in bookingItem.TimeSlots)
                    {
                        if (!TimeSpan.TryParse(timeSlot.StartTime, out var startTime) || !TimeSpan.TryParse(timeSlot.EndTime, out var endTime))
                            throw new ArgumentException("Thời gian không hợp lệ.");

                        if (startTime >= endTime)
                            throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

                        if (startTime < subField.OpenTime || endTime > subField.CloseTime)
                            throw new ArgumentException("Thời gian đặt sân ngoài khung giờ hoạt động.");

                        var isAvailable = await CheckTimeSlotAvailabilityAsync(bookingItem.SubFieldId, bookingItem.BookingDate, startTime, endTime);
                        if (!isAvailable)
                            throw new ArgumentException($"Khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã được đặt.");

                        var slotPrice = await CalculateBasePriceAsync(subField, bookingItem.BookingDate, startTime, endTime);
                        subTotal += slotPrice;

                        timeSlots.Add(new BookingTimeSlot
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Price = slotPrice
                        });
                    }

                    decimal servicePrice = 0;
                    var bookingServices = new List<Models.BookingService>();
                    if (bookingItem.Services != null)
                    {
                        foreach (var service in bookingItem.Services)
                        {
                            var fieldService = await _unitOfWork.Repository<Models.FieldService>()
                                .FindSingleAsync(fs => fs.FieldServiceId == service.FieldServiceId && fs.FieldId == subField.FieldId && fs.IsActive && fs.DeletedAt == null);
                            if (fieldService == null)
                                throw new ArgumentException($"Dịch vụ {service.FieldServiceId} không tồn tại hoặc không khả dụng.");

                            servicePrice += fieldService.Price * service.Quantity;
                            bookingServices.Add(new Models.BookingService
                            {
                                FieldServiceId = service.FieldServiceId,
                                Quantity = service.Quantity,
                                Price = fieldService.Price * service.Quantity
                            });
                        }
                    }

                    subTotal += servicePrice;

                    var booking = new api.Models.Booking
                    {
                        UserId = userEntity.UserId,
                        SubFieldId = bookingItem.SubFieldId,
                        MainBookingId = mainBooking.BookingId,
                        BookingDate = bookingItem.BookingDate,
                        TotalPrice = subTotal,
                        Status = "Pending",
                        PaymentStatus = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        TimeSlots = timeSlots,
                        BookingServices = bookingServices
                    };

                    if (!string.IsNullOrEmpty(request.PromotionCode))
                    {
                        var promotion = await _unitOfWork.Repository<Promotion>()
                            .FindSingleAsync(p => p.Code == request.PromotionCode && p.IsActive && p.DeletedAt == null &&
                                                 p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow &&
                                                 (p.FieldId == null || p.FieldId == subField.FieldId));
                        if (promotion != null && subTotal >= (promotion.MinBookingValue ?? 0))
                        {
                            var slotDiscount = promotion.DiscountType == "Percentage"
                                ? subTotal * (promotion.DiscountValue ?? 0) / 100
                                : promotion.DiscountValue ?? 0;
                            if (promotion.MaxDiscountAmount.HasValue && slotDiscount > promotion.MaxDiscountAmount.Value)
                                slotDiscount = promotion.MaxDiscountAmount.Value;
                            discount += slotDiscount;
                            booking.PromotionId = promotion.PromotionId;
                        }
                    }

                    await _unitOfWork.Repository<api.Models.Booking>().AddAsync(booking);
                    await _unitOfWork.SaveChangesAsync();
                    totalPrice += subTotal;

                    response.Bookings.Add(new BookingItemResponseDto
                    {
                        BookingId = booking.BookingId,
                        SubFieldId = booking.SubFieldId,
                        SubFieldName = subField.SubFieldName,
                        FieldName = subField.Field.FieldName,
                        BookingDate = booking.BookingDate,
                        TimeSlots = booking.TimeSlots.Select(ts => new TimeSlotResponseDto
                        {
                            StartTime = ts.StartTime.ToString(@"hh\:mm"),
                            EndTime = ts.EndTime.ToString(@"hh\:mm"),
                            Price = ts.Price
                        }).ToList(),
                        Services = booking.BookingServices.Select(bs => new BookingServiceResponseDto
                        {
                            FieldServiceId = bs.FieldServiceId,
                            ServiceName = bs.FieldService.ServiceName,
                            Quantity = bs.Quantity,
                            Price = bs.Price
                        }).ToList(),
                        SubTotal = subTotal,
                        Status = booking.Status,
                        PaymentStatus = booking.PaymentStatus
                    });
                }

                mainBooking.TotalPrice = totalPrice - discount;
                _unitOfWork.Repository<api.Models.Booking>().Update(mainBooking);
                await _unitOfWork.SaveChangesAsync();

                response.TotalPrice = totalPrice;
                response.Discount = discount;
                response.FinalPrice = totalPrice - discount;

                // Tạo thanh toán và lấy PaymentUrl
                var paymentRequest = new CreatePaymentRequestDto
                {
                    MainBookingId = mainBooking.BookingId,
                    Amount = response.FinalPrice,
                    PaymentMethod = "VNPay" // Giả định sử dụng VNPay
                };
                var paymentResponse = await _paymentService.CreatePaymentAsync(user, paymentRequest);
                response.PaymentUrl = paymentResponse.PaymentUrl;
                response.Message = "Đặt sân thành công";

                await transaction.CommitAsync();
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AddBookingServiceResponseDto> AddBookingServiceAsync(ClaimsPrincipal user, int bookingId, AddBookingServiceRequestDto request)
        {
            _logger.LogInformation("Thêm dịch vụ cho đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể thêm dịch vụ.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.UserId == userEntity.UserId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại hoặc bạn không có quyền truy cập.");

            var fieldService = await _unitOfWork.Repository<Models.FieldService>()
                .FindSingleAsync(fs => fs.FieldServiceId == request.FieldServiceId && fs.FieldId == booking.SubField.FieldId && fs.IsActive && fs.DeletedAt == null);
            if (fieldService == null)
                throw new ArgumentException("Dịch vụ không tồn tại hoặc không khả dụng.");

            var bookingService = new Models.BookingService
            {
                BookingId = bookingId,
                FieldServiceId = request.FieldServiceId,
                Quantity = request.Quantity,
                Price = fieldService.Price * request.Quantity
            };

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Repository<Models.BookingService>().AddAsync(bookingService);
                booking.TotalPrice += bookingService.Price;
                _unitOfWork.Repository<api.Models.Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new AddBookingServiceResponseDto
                {
                    BookingServiceId = bookingService.BookingServiceId,
                    BookingId = bookingId,
                    FieldServiceId = request.FieldServiceId,
                    ServiceName = fieldService.ServiceName,
                    Quantity = request.Quantity,
                    Price = bookingService.Price,
                    Message = "Thêm dịch vụ thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BookingDetailsResponseDto> GetBookingByIdAsync(ClaimsPrincipal user, int bookingId)
        {
            _logger.LogInformation("Lấy chi tiết đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.TimeSlots)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.FieldService);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại.");

            bool hasAccess = false;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity != null && booking.UserId == userEntity.UserId)
                    hasAccess = true;
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity != null && booking.SubField.Field.OwnerId == ownerEntity.OwnerId)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException("Bạn không có quyền xem đặt sân này.");

            return new BookingDetailsResponseDto
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                SubFieldId = booking.SubFieldId,
                SubFieldName = booking.SubField.SubFieldName,
                FieldName = booking.SubField.Field.FieldName,
                BookingDate = booking.BookingDate,
                TimeSlots = booking.TimeSlots.Select(ts => new TimeSlotResponseDto
                {
                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
                    Price = ts.Price
                }).ToList(),
                Services = booking.BookingServices.Select(bs => new BookingServiceResponseDto
                {
                    FieldServiceId = bs.FieldServiceId,
                    ServiceName = bs.FieldService.ServiceName,
                    Quantity = bs.Quantity,
                    Price = bs.Price
                }).ToList(),
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                Message = "Lấy chi tiết đặt sân thành công"
            };
        }

        public async Task<UserBookingsResponseDto> GetUserBookingsAsync(ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách đặt sân của người dùng");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể xem danh sách đặt sân.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            IQueryable<api.Models.Booking> query = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.UserId == userEntity.UserId && b.DeletedAt == null && b.MainBookingId == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.TimeSlots);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate <= endDate.Value);

            query = query.OrderByDescending(b => b.BookingDate);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingSummaryDto
                {
                    BookingId = b.BookingId,
                    FieldName = b.SubField.Field.FieldName,
                    SubFieldName = b.SubField.SubFieldName,
                    BookingDate = b.BookingDate,
                    StartTime = b.TimeSlots.OrderBy(ts => ts.StartTime).Select(ts => ts.StartTime).FirstOrDefault().ToString(@"hh\:mm"),
                    EndTime = b.TimeSlots.OrderByDescending(ts => ts.EndTime).Select(ts => ts.EndTime).FirstOrDefault().ToString(@"hh\:mm"),
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus
                })
                .ToListAsync();

            return new UserBookingsResponseDto
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<BookingServicesResponseDto> GetBookingServicesAsync(ClaimsPrincipal user, int bookingId, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách dịch vụ của đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại.");

            bool hasAccess = false;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity != null && booking.UserId == userEntity.UserId)
                    hasAccess = true;
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity != null && booking.SubField.Field.OwnerId == ownerEntity.OwnerId)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException("Bạn không có quyền xem dịch vụ này.");

            var query = _unitOfWork.Repository<Models.BookingService>()
                .FindQueryable(bs => bs.BookingId == bookingId)
                .Include(bs => bs.FieldService);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bs => new BookingServiceResponseDto
                {
                    FieldServiceId = bs.FieldServiceId,
                    ServiceName = bs.FieldService.ServiceName,
                    Quantity = bs.Quantity,
                    Price = bs.Price
                })
                .ToListAsync();

            return new BookingServicesResponseDto
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UpdateBookingResponseDto> UpdateBookingAsync(ClaimsPrincipal user, int bookingId, UpdateBookingRequestDto request)
        {
            _logger.LogInformation("Cập nhật đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "Owner")
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể cập nhật đặt sân.");

            var ownerEntity = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (ownerEntity == null)
                throw new ArgumentException("Chủ sân không tồn tại.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại.");

            if (booking.SubField.Field.OwnerId != ownerEntity.OwnerId)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đặt sân này.");

            booking.Status = request.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<api.Models.Booking>().Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateBookingResponseDto
            {
                BookingId = booking.BookingId,
                Status = booking.Status,
                Message = "Cập nhật đặt sân thành công"
            };
        }

        public async Task<ConfirmBookingResponseDto> ConfirmBookingAsync(ClaimsPrincipal user, int bookingId)
        {
            _logger.LogInformation("Xác nhận đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "Owner")
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể xác nhận đặt sân.");

            var ownerEntity = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (ownerEntity == null)
                throw new ArgumentException("Chủ sân không tồn tại.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại.");

            if (booking.SubField.Field.OwnerId != ownerEntity.OwnerId)
                throw new UnauthorizedAccessException("Bạn không có quyền xác nhận đặt sân này.");

            if (booking.Status != "Pending")
                throw new ArgumentException("Chỉ có thể xác nhận đặt sân đang ở trạng thái Pending.");

            booking.Status = "Confirmed";
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<api.Models.Booking>().Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return new ConfirmBookingResponseDto
            {
                BookingId = booking.BookingId,
                Status = booking.Status,
                Message = "Xác nhận đặt sân thành công"
            };
        }

        public async Task<RescheduleBookingResponseDto> RescheduleBookingAsync(ClaimsPrincipal user, int bookingId, RescheduleBookingRequestDto request)
        {
            _logger.LogInformation("Đổi lịch đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể đổi lịch đặt sân.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.UserId == userEntity.UserId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                .Include(b => b.TimeSlots);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại hoặc bạn không có quyền truy cập.");

            if (booking.Status == "Cancelled")
                throw new ArgumentException("Không thể đổi lịch cho đặt sân đã hủy.");

            if (!TimeSpan.TryParse(request.NewStartTime, out var newStartTime) || !TimeSpan.TryParse(request.NewEndTime, out var newEndTime))
                throw new ArgumentException("Thời gian không hợp lệ.");

            if (newStartTime >= newEndTime)
                throw new ArgumentException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

            if (newStartTime < booking.SubField.OpenTime || newEndTime > booking.SubField.CloseTime)
                throw new ArgumentException("Thời gian đặt sân ngoài khung giờ hoạt động.");

            var isAvailable = await CheckTimeSlotAvailabilityAsync(booking.SubFieldId, request.NewDate, newStartTime, newEndTime);
            if (!isAvailable)
                throw new ArgumentException("Khung giờ mới đã được đặt.");

            decimal newPrice = await CalculateBasePriceAsync(booking.SubField, request.NewDate, newStartTime, newEndTime);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                booking.BookingDate = request.NewDate;
                booking.TotalPrice = newPrice + booking.BookingServices.Sum(bs => bs.Price);
                booking.UpdatedAt = DateTime.UtcNow;

                foreach (var timeSlot in booking.TimeSlots)
                {
                    timeSlot.StartTime = newStartTime;
                    timeSlot.EndTime = newEndTime;
                    timeSlot.Price = newPrice;
                    _unitOfWork.Repository<BookingTimeSlot>().Update(timeSlot);
                }

                _unitOfWork.Repository<api.Models.Booking>().Update(booking);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RescheduleBookingResponseDto
                {
                    BookingId = booking.BookingId,
                    BookingDate = booking.BookingDate,
                    StartTime = request.NewStartTime,
                    EndTime = request.NewEndTime,
                    Message = "Đổi lịch đặt sân thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CancelBookingResponseDto> CancelBookingAsync(ClaimsPrincipal user, int bookingId)
        {
            _logger.LogInformation("Hủy đặt sân BookingId: {BookingId}", bookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            var bookingQuery = _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.BookingId == bookingId && b.DeletedAt == null)
                .Include(b => b.SubField).ThenInclude(sf => sf.Field);

            var booking = await bookingQuery.FirstOrDefaultAsync();
            if (booking == null)
                throw new ArgumentException("Đặt sân không tồn tại.");

            bool hasAccess = false;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity != null && booking.UserId == userEntity.UserId)
                    hasAccess = true;
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity != null && booking.SubField.Field.OwnerId == ownerEntity.OwnerId)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException("Bạn không có quyền hủy đặt sân này.");

            if (booking.Status == "Cancelled")
                throw new ArgumentException("Đặt sân đã bị hủy.");

            if (booking.BookingDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Không thể hủy đặt sân đã qua.");

            booking.Status = "Cancelled";
            booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<api.Models.Booking>().Update(booking);
            await _unitOfWork.SaveChangesAsync();

            return new CancelBookingResponseDto
            {
                BookingId = booking.BookingId,
                Status = booking.Status,
                Message = "Hủy đặt sân thành công"
            };
        }

        private async Task<bool> CheckTimeSlotAvailabilityAsync(int subFieldId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
        {
            var existingBookings = await _unitOfWork.Repository<api.Models.Booking>()
                .FindQueryable(b => b.SubFieldId == subFieldId && b.BookingDate == bookingDate && b.Status != "Cancelled" && b.DeletedAt == null)
                .Include(b => b.TimeSlots)
                .ToListAsync();

            foreach (var booking in existingBookings)
            {
                foreach (var timeSlot in booking.TimeSlots)
                {
                    if (!(endTime <= timeSlot.StartTime || startTime >= timeSlot.EndTime))
                        return false;
                }
            }
            return true;
        }

        private async Task<decimal> CalculateBasePriceAsync(SubField subField, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
        {
            decimal totalPrice = 0;
            var currentTime = startTime;
            var dayOfWeek = (int)bookingDate.DayOfWeek;

            while (currentTime < endTime)
            {
                var nextTime = currentTime.Add(TimeSpan.FromMinutes(30));
                var pricing = await _unitOfWork.Repository<FieldPricing>()
                    .FindSingleAsync(p => p.SubFieldId == subField.SubFieldId && p.DayOfWeek == dayOfWeek &&
                                         p.StartTime <= currentTime && p.EndTime >= nextTime && p.IsActive && p.DeletedAt == null);
                totalPrice += pricing?.Price ?? subField.DefaultPricePerSlot;
                currentTime = nextTime;
            }

            return totalPrice;
        }
    }
}