using System;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Payment;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET;

namespace api.Controllers
{
    /// <summary>
    /// Controller xử lý các yêu cầu liên quan đến thanh toán.
    /// </summary>
    [Route("api/payments")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVnpay _vnpay;
        private readonly ILogger<PaymentController> _logger;
        private readonly string _frontendUrl;

        /// <summary>
        /// Khởi tạo PaymentController với các dependencies cần thiết.
        /// </summary>
        /// <param name="paymentService">Service xử lý logic thanh toán.</param>
        /// <param name="vnpay">Service tích hợp cổng thanh toán VNPay.</param>
        /// <param name="logger">Logger để ghi log hệ thống.</param>
        /// <param name="configuration">Configuration để lấy thông tin cấu hình.</param>
        public PaymentController(
            IPaymentService paymentService,
            IVnpay vnpay,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _vnpay = vnpay;
            _logger = logger;
            _frontendUrl = configuration["FEUrl"] ?? "http://localhost:5173";
        }

        /// <summary>
        /// Khởi tạo thanh toán cho một hoặc nhiều đặt sân liên kết bởi mainBookingId.
        /// </summary>
        /// <param name="request">Thông tin thanh toán cần tạo.</param>
        /// <returns>URL thanh toán để người dùng hoàn tất giao dịch.</returns>
        /// <response code="201">Tạo thanh toán thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền tạo thanh toán.</response>
        /// <response code="404">Không tìm thấy đặt sân.</response>
        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CreatePaymentResponseDto>> CreatePayment([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(User, request);
                return CreatedAtAction(nameof(GetPaymentStatus), new { paymentId = payment.PaymentId }, payment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thanh toán cho MainBookingId: {MainBookingId}", request.MainBookingId);
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "general", message = ex.Message } } });
            }
        }

        /// <summary>
        /// Lấy trạng thái của một thanh toán.
        /// </summary>
        /// <param name="paymentId">ID của thanh toán cần lấy trạng thái.</param>
        /// <returns>Thông tin trạng thái thanh toán.</returns>
        /// <response code="200">Trả về trạng thái thanh toán.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xem thanh toán.</response>
        /// <response code="404">Không tìm thấy thanh toán.</response>
        [HttpGet("{paymentId}")]
        [Authorize(Policy = "UserOnly,OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(int paymentId)
        {
            try
            {
                var paymentStatus = await _paymentService.GetPaymentStatusAsync(User, paymentId);
                return Ok(paymentStatus);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ex.Message.Contains("Token") ? Unauthorized(new { error = "Unauthorized", message = ex.Message }) 
                                                   : Forbid("Only the booking user or field owner can view payment");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = "Resource not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy trạng thái thanh toán PaymentId: {PaymentId}", paymentId);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử thanh toán của người dùng hoặc chủ sân.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (Pending, Completed, Failed).</param>
        /// <param name="startDate">Ngày bắt đầu lọc.</param>
        /// <param name="endDate">Ngày kết thúc lọc.</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách lịch sử thanh toán.</returns>
        /// <response code="200">Trả về lịch sử thanh toán.</response>
        /// <response code="400">Tham số không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        [HttpGet]
        [Authorize(Policy = "UserOnly,OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentHistoryResponseDto>> GetPaymentHistory(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var history = await _paymentService.GetPaymentHistoryAsync(User, status, startDate, endDate, page, pageSize);
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "general", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử thanh toán");
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý webhook từ cổng thanh toán.
        /// </summary>
        /// <param name="request">Thông tin cập nhật từ cổng thanh toán.</param>
        /// <param name="vnp_SecureHash">Chữ ký xác thực từ cổng thanh toán.</param>
        /// <returns>Kết quả xử lý webhook.</returns>
        /// <response code="200">Xử lý webhook thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chữ ký không hợp lệ.</response>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ProcessWebhook([FromBody] WebhookRequestDto request, [FromQuery] string vnp_SecureHash)
        {
            try
            {
                var result = await _paymentService.ProcessWebhookAsync(request, vnp_SecureHash);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Invalid input", details = new[] { new { field = "paymentId", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý webhook PaymentId: {PaymentId}", request.PaymentId);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
        }

        /// <summary>
        /// Yêu cầu hoàn tiền cho một đặt sân.
        /// </summary>
        /// <param name="paymentId">ID của thanh toán cần hoàn tiền.</param>
        /// <param name="request">Thông tin yêu cầu hoàn tiền.</param>
        /// <returns>Thông tin yêu cầu hoàn tiền đã tạo.</returns>
        /// <response code="201">Tạo yêu cầu hoàn tiền thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền yêu cầu hoàn tiền.</response>
        /// <response code="404">Không tìm thấy thanh toán.</response>
        [HttpPost("{paymentId}/refund")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestRefund(int paymentId, [FromBody] RefundRequestDto request)
        {
            try
            {
                var refund = await _paymentService.RequestRefundAsync(User, paymentId, request);
                return CreatedAtAction(nameof(GetPaymentStatus), new { paymentId }, refund);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ex.Message.Contains("Token") ? Unauthorized(new { error = "Unauthorized", message = ex.Message }) 
                                                   : Forbid("Only the booking user can request a refund");
            }
            catch (ArgumentException ex)
            {
                return ex.Message.Contains("không tồn tại") ? NotFound(new { error = "Resource not found", message = ex.Message })
                                                           : BadRequest(new { error = "Invalid input", details = new[] { new { field = "amount", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi yêu cầu hoàn tiền PaymentId: {PaymentId}", paymentId);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý yêu cầu hoàn tiền (chỉ dành cho chủ sân hoặc admin).
        /// </summary>
        /// <param name="refundId">ID của yêu cầu hoàn tiền cần xử lý.</param>
        /// <param name="request">Thông tin xử lý yêu cầu hoàn tiền.</param>
        /// <returns>Thông tin yêu cầu hoàn tiền sau khi xử lý.</returns>
        /// <response code="200">Xử lý yêu cầu hoàn tiền thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
        /// <response code="403">Không có quyền xử lý yêu cầu hoàn tiền.</response>
        /// <response code="404">Không tìm thấy yêu cầu hoàn tiền.</response>
        [HttpPost("refunds/{refundId}/process")]
        [Authorize(Policy = "OwnerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessRefund(int refundId, [FromBody] ProcessRefundRequestDto request)
        {
            try
            {
                var refund = await _paymentService.ProcessRefundAsync(User, refundId, request);
                return Ok(refund);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ex.Message.Contains("Token") ? Unauthorized(new { error = "Unauthorized", message = ex.Message }) 
                                                   : Forbid("Only the field owner or admin can process refunds");
            }
            catch (ArgumentException ex)
            {
                return ex.Message.Contains("không tồn tại") ? NotFound(new { error = "Resource not found", message = ex.Message })
                                                           : BadRequest(new { error = "Invalid input", details = new[] { new { field = "status", message = ex.Message } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý hoàn tiền RefundId: {RefundId}", refundId);
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý callback từ cổng thanh toán VNPay.
        /// </summary>
        /// <returns>Chuyển hướng đến trang kết quả thanh toán.</returns>
        /// <response code="200">Xử lý callback thành công.</response>
        /// <response code="400">Dữ liệu callback không hợp lệ.</response>
        [HttpGet("return")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PaymentReturn()
        {
            try
            {
                _logger.LogInformation("Xử lý callback từ VNPay");

                if (!Request.QueryString.HasValue)
                    return BadRequest(new { error = "Invalid query", message = "Không tìm thấy thông tin thanh toán" });

                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";

                if (paymentResult.IsSuccess)
                {
                    return Redirect($"{_frontendUrl}/payment/success?paymentId={paymentResult.PaymentId}");
                }

                return Redirect($"{_frontendUrl}/payment/failed?paymentId={paymentResult.PaymentId}&message={Uri.EscapeDataString(resultDescription)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý callback VNPay");
                return BadRequest(new { error = "Processing error", message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý thông báo thanh toán trực tuyến (IPN) từ cổng thanh toán VNPay.
        /// </summary>
        /// <returns>Kết quả xử lý IPN.</returns>
        /// <response code="200">Xử lý IPN thành công.</response>
        /// <response code="400">Dữ liệu IPN không hợp lệ.</response>
        [HttpGet("ipn")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Ipn()
        {
            try
            {
                _logger.LogInformation("Xử lý IPN từ VNPay");

                if (!Request.QueryString.HasValue)
                    return BadRequest(new { error = "Invalid query", message = "Không tìm thấy thông tin thanh toán" });

                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    // Lấy Amount từ vnp_Amount (chia 100 để chuyển về VND)
                    double amount = double.Parse(Request.Query["vnp_Amount"]) / 100;

                    // Gọi webhook để cập nhật trạng thái
                    var webhookRequest = new WebhookRequestDto
                    {
                        PaymentId = (int)paymentResult.PaymentId,
                        Status = "Completed",
                        TransactionId = paymentResult.VnpayTransactionId.ToString(),
                        Amount = amount,
                        Timestamp = paymentResult.Timestamp
                    };
                    _paymentService.ProcessWebhookAsync(webhookRequest, Request.Query["vnp_SecureHash"]).GetAwaiter().GetResult();
                    return Ok(new { message = "IPN processed successfully" });
                }

                return BadRequest(new { error = "Payment failed", message = paymentResult.PaymentResponse.Description });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý IPN VNPay");
                return BadRequest(new { error = "Processing error", message = ex.Message });
            }
        }
    }
}