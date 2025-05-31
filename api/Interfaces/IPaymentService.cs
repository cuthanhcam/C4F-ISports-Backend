using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Payment;

namespace api.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(ClaimsPrincipal user, CreatePaymentRequestDto request);
        Task<PaymentStatusResponseDto> GetPaymentStatusAsync(ClaimsPrincipal user, int paymentId);
        Task<PaymentHistoryResponseDto> GetPaymentHistoryAsync(ClaimsPrincipal user, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<WebhookResponseDto> ProcessWebhookAsync(WebhookRequestDto request, string secureHash);
        Task<RefundResponseDto> RequestRefundAsync(ClaimsPrincipal user, int paymentId, RefundRequestDto request);
        Task<ProcessRefundResponseDto> ProcessRefundAsync(ClaimsPrincipal user, int refundId, ProcessRefundRequestDto request);
    }
}