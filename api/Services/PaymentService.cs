using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Payment;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VNPAY.NET; // Namespace đúng
using VNPAY.NET.Enums;
using VNPAY.NET.Models; // Để sử dụng PaymentRequest, Currency, DisplayLanguage

namespace api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IVnpay _vnpay; // Sử dụng IVnpay thay vì VNPayLibrary
        private readonly VNPaySettings _vnpaySettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IAuthService authService,
            ILogger<PaymentService> logger,
            IVnpay vnpay, // Sử dụng IVnpay
            IOptions<VNPaySettings> vnpaySettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _logger = logger;
            _vnpay = vnpay;
            _vnpaySettings = vnpaySettings.Value;
            _httpContextAccessor = httpContextAccessor;

            // Khởi tạo VNPay
            _vnpay.Initialize(_vnpaySettings.TmnCode, _vnpaySettings.HashSecret, _vnpaySettings.BaseUrl, _vnpaySettings.ReturnUrl);
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(ClaimsPrincipal user, CreatePaymentRequestDto request)
        {
            _logger.LogInformation("Tạo thanh toán cho MainBookingId: {MainBookingId}", request.MainBookingId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể tạo thanh toán.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            var mainBooking = await _unitOfWork.Repository<Booking>()
                .FindQueryable(b => b.BookingId == request.MainBookingId && b.UserId == userEntity.UserId && b.DeletedAt == null)
                .Include(b => b.RelatedBookings)
                .FirstOrDefaultAsync();
            if (mainBooking == null)
                throw new ArgumentException("Booking không tồn tại hoặc bạn không có quyền truy cập.");

            var totalBookingAmount = mainBooking.TotalPrice + mainBooking.RelatedBookings.Sum(b => b.TotalPrice);
            if (request.Amount != totalBookingAmount)
                throw new ArgumentException("Số tiền thanh toán không khớp với tổng giá trị booking.");

            if (mainBooking.PaymentStatus == "Paid")
                throw new ArgumentException("Booking đã được thanh toán.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payment = new Payment
                {
                    BookingId = request.MainBookingId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    Status = "Pending",
                    Currency = "VND",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                // Tạo URL thanh toán VNPay
                var paymentRequest = new PaymentRequest
                {
                    PaymentId = payment.PaymentId,
                    Money = (double)request.Amount,
                    Description = $"Thanh toán cho booking {request.MainBookingId}",
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(paymentRequest);

                payment.PaymentUrl = paymentUrl;
                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreatePaymentResponseDto
                {
                    PaymentId = payment.PaymentId,
                    MainBookingId = request.MainBookingId,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    PaymentUrl = payment.PaymentUrl,
                    Message = "Tạo thanh toán thành công"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo thanh toán cho MainBookingId: {MainBookingId}", request.MainBookingId);
                throw;
            }
        }

        public async Task<PaymentStatusResponseDto> GetPaymentStatusAsync(ClaimsPrincipal user, int paymentId)
        {
            _logger.LogInformation("Lấy trạng thái thanh toán PaymentId: {PaymentId}", paymentId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            var payment = await _unitOfWork.Repository<Payment>()
                .FindQueryable(p => p.PaymentId == paymentId && p.DeletedAt == null)
                .Include(p => p.Booking).ThenInclude(b => b.SubField).ThenInclude(sf => sf.Field)
                .FirstOrDefaultAsync();
            if (payment == null)
                throw new ArgumentException("Thanh toán không tồn tại.");

            bool hasAccess = false;
            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity != null && payment.Booking.UserId == userEntity.UserId)
                    hasAccess = true;
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity != null && payment.Booking.SubField.Field.OwnerId == ownerEntity.OwnerId)
                    hasAccess = true;
            }

            if (!hasAccess)
                throw new UnauthorizedAccessException("Bạn không có quyền xem thanh toán này.");

            return new PaymentStatusResponseDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }

        public async Task<PaymentHistoryResponseDto> GetPaymentHistoryAsync(ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            _logger.LogInformation("Lấy lịch sử thanh toán");
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
                throw new UnauthorizedAccessException("Token không hợp lệ.");

            IQueryable<Payment> query;

            if (account.Role == "User")
            {
                var userEntity = await _unitOfWork.Repository<User>()
                    .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
                if (userEntity == null)
                    throw new ArgumentException("Người dùng không tồn tại.");

                query = _unitOfWork.Repository<Payment>()
                    .FindQueryable(p => p.Booking.UserId == userEntity.UserId && p.DeletedAt == null)
                    .Include(p => p.Booking);
            }
            else if (account.Role == "Owner")
            {
                var ownerEntity = await _unitOfWork.Repository<Owner>()
                    .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
                if (ownerEntity == null)
                    throw new ArgumentException("Chủ sân không tồn tại.");

                query = _unitOfWork.Repository<Payment>()
                    .FindQueryable(p => p.Booking.SubField.Field.OwnerId == ownerEntity.OwnerId && p.DeletedAt == null)
                    .Include(p => p.Booking).ThenInclude(b => b.SubField).ThenInclude(sf => sf.Field);
            }
            else
            {
                throw new UnauthorizedAccessException("Vai trò không hợp lệ.");
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            query = query.OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentSummaryDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return new PaymentHistoryResponseDto
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<WebhookResponseDto> ProcessWebhookAsync(WebhookRequestDto request, string secureHash)
        {
            _logger.LogInformation("Xử lý webhook thanh toán PaymentId: {PaymentId}", request.PaymentId);

            // Tạo query string giả lập để xác minh
            var queryParams = new Dictionary<string, string>
            {
                { "vnp_TxnRef", request.PaymentId.ToString() },
                { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
                { "vnp_TransactionNo", request.TransactionId },
                { "vnp_ResponseCode", request.Status == "Completed" ? "00" : "01" },
                { "vnp_PayDate", request.Timestamp.ToString("yyyyMMddHHmmss") }
            };
            
            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            // Xác minh chữ ký VNPay
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?" + query);
            var paymentResult = _vnpay.GetPaymentResult(httpContext.Request.Query);
            if (!paymentResult.IsSuccess)
                throw new UnauthorizedAccessException("Chữ ký webhook không hợp lệ hoặc giao dịch thất bại.");

            var payment = await _unitOfWork.Repository<Payment>()
                .FindQueryable(p => p.PaymentId == request.PaymentId && p.DeletedAt == null)
                .Include(p => p.Booking)
                .FirstOrDefaultAsync();
            if (payment == null)
                throw new ArgumentException("Thanh toán không tồn tại.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                payment.Status = paymentResult.IsSuccess ? "Success" : "Failed";
                payment.TransactionId = paymentResult.VnpayTransactionId.ToString();
                payment.PaymentDate = paymentResult.Timestamp;

                payment.Booking.PaymentStatus = payment.Status == "Success" ? "Paid" : "Failed";
                _unitOfWork.Repository<Payment>().Update(payment);
                _unitOfWork.Repository<Booking>().Update(payment.Booking);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new WebhookResponseDto
                {
                    Message = "Xử lý webhook thành công"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý webhook cho PaymentId: {PaymentId}", request.PaymentId);
                throw;
            }
        }

        public async Task<RefundResponseDto> RequestRefundAsync(ClaimsPrincipal user, int paymentId, RefundRequestDto request)
        {
            _logger.LogInformation("Yêu cầu hoàn tiền cho PaymentId: {PaymentId}", paymentId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "User")
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể yêu cầu hoàn tiền.");

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            var payment = await _unitOfWork.Repository<Payment>()
                .FindQueryable(p => p.PaymentId == paymentId && p.DeletedAt == null)
                .Include(p => p.Booking)
                .FirstOrDefaultAsync();
            if (payment == null)
                throw new ArgumentException("Thanh toán không tồn tại.");

            if (payment.Booking.UserId != userEntity.UserId)
                throw new UnauthorizedAccessException("Bạn không có quyền yêu cầu hoàn tiền cho thanh toán này.");

            if (payment.Status != "Success")
                throw new ArgumentException("Chỉ có thể yêu cầu hoàn tiền cho thanh toán đã hoàn tất.");

            if (request.Amount > payment.Amount)
                throw new ArgumentException("Số tiền hoàn không được vượt quá số tiền thanh toán.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var refundRequest = new RefundRequest
                {
                    PaymentId = paymentId,
                    Amount = request.Amount,
                    Reason = request.Reason,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<RefundRequest>().AddAsync(refundRequest);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RefundResponseDto
                {
                    RefundId = refundRequest.RefundRequestId,
                    PaymentId = paymentId,
                    Amount = refundRequest.Amount,
                    Status = refundRequest.Status,
                    Message = "Yêu cầu hoàn tiền thành công"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi yêu cầu hoàn tiền cho PaymentId: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<ProcessRefundResponseDto> ProcessRefundAsync(ClaimsPrincipal user, int refundId, ProcessRefundRequestDto request)
        {
            _logger.LogInformation("Xử lý yêu cầu hoàn tiền RefundId: {RefundId}", refundId);
            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null || account.Role != "Owner")
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể xử lý yêu cầu hoàn tiền.");

            var ownerEntity = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (ownerEntity == null)
                throw new ArgumentException("Chủ sân không tồn tại.");

            var refundRequest = await _unitOfWork.Repository<RefundRequest>()
                .FindQueryable(r => r.RefundRequestId == refundId && r.DeletedAt == null)
                .Include(r => r.Payment).ThenInclude(p => p.Booking).ThenInclude(b => b.SubField).ThenInclude(sf => sf.Field)
                .FirstOrDefaultAsync();
            if (refundRequest == null)
                throw new ArgumentException("Yêu cầu hoàn tiền không tồn tại.");

            if (refundRequest.Payment.Booking.SubField.Field.OwnerId != ownerEntity.OwnerId)
                throw new UnauthorizedAccessException("Bạn không có quyền xử lý yêu cầu hoàn tiền này.");

            if (refundRequest.Status != "Pending")
                throw new ArgumentException("Chỉ có thể xử lý yêu cầu hoàn tiền ở trạng thái Pending.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                refundRequest.Status = request.Status;
                refundRequest.Note = request.Note;
                refundRequest.UpdatedAt = DateTime.UtcNow;

                if (request.Status == "Approved")
                {
                    refundRequest.Payment.Status = "Refunded";
                    refundRequest.Payment.Booking.PaymentStatus = "Refunded";
                    _unitOfWork.Repository<Payment>().Update(refundRequest.Payment);
                    _unitOfWork.Repository<Booking>().Update(refundRequest.Payment.Booking);
                }

                _unitOfWork.Repository<RefundRequest>().Update(refundRequest);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ProcessRefundResponseDto
                {
                    RefundId = refundRequest.RefundRequestId,
                    Status = refundRequest.Status,
                    Message = "Xử lý yêu cầu hoàn tiền thành công"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý yêu cầu hoàn tiền RefundId: {RefundId}", refundId);
                throw;
            }
        }
    }
}