using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class BookingReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Kiểm tra mỗi 15 phút

        public BookingReminderService(IServiceProvider serviceProvider, ILogger<BookingReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending booking reminders. StackTrace: {StackTrace}", ex.StackTrace);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Booking Reminder Service is stopping.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                // Thời gian hiện tại theo múi giờ UTC+7
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, 
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                try
                {
                    var bookings = await unitOfWork.Bookings.GetAll()
                        .Include(b => b.User)
                        .Include(b => b.SubField).ThenInclude(sf => sf.Field)
                        .Where(b => b.Status == "Confirmed" && 
                                    b.MainBookingId == null && 
                                    b.IsReminderSent == false) // Chỉ lấy booking chưa gửi nhắc nhở
                        .ToListAsync();

                    _logger.LogInformation("Retrieved {Count} bookings from DB at {Now}", bookings.Count, now);

                    var filteredBookings = bookings
                        .Where(b => b.BookingDate != null && b.StartTime != null &&
                                    (b.BookingDate + b.StartTime - now).TotalMinutes > 0 && 
                                    (b.BookingDate + b.StartTime - now).TotalMinutes <= 60) // 60 phút trước
                        .ToList();

                    _logger.LogInformation("Found {Count} bookings to remind at {Now}", filteredBookings.Count, now);

                    foreach (var booking in filteredBookings)
                    {
                        if (booking.User == null || string.IsNullOrEmpty(booking.User.Email))
                        {
                            _logger.LogWarning("User or email not found for BookingId: {BookingId}", booking.BookingId);
                            continue;
                        }

                        var subject = "Nhắc nhở lịch đặt sân của bạn";
                        var htmlMessage = $@"
                            <h3>Xin chào {booking.User.FullName},</h3>
                            <p>Chúng tôi xin nhắc bạn về lịch đặt sân sắp tới:</p>
                            <ul>
                                <li><strong>Sân:</strong> {(booking.SubField?.Field?.FieldName ?? "Unknown")} - {(booking.SubField?.SubFieldName ?? "Unknown")}</li>
                                <li><strong>Ngày:</strong> {booking.BookingDate:yyyy-MM-dd}</li>
                                <li><strong>Thời gian:</strong> {booking.StartTime} - {booking.EndTime}</li>
                                <li><strong>Tổng giá:</strong> {booking.TotalPrice:N0} VND</li>
                                <li><strong>Trạng thái thanh toán:</strong> {booking.PaymentStatus}</li>
                            </ul>
                            <p>Vui lòng đến đúng giờ. Chúc bạn có một trận đấu tuyệt vời!</p>
                            <p>Trân trọng,<br/>Đội ngũ hỗ trợ</p>";

                        try
                        {
                            await emailSender.SendEmailAsync(booking.User.Email, subject, htmlMessage);
                            _logger.LogInformation("Sent reminder email for BookingId: {BookingId} to {Email}", booking.BookingId, booking.User.Email);

                            // Cập nhật IsReminderSent thành true
                            booking.IsReminderSent = true;
                            unitOfWork.Bookings.Update(booking);
                            await unitOfWork.SaveChangesAsync();
                            _logger.LogInformation("Updated IsReminderSent for BookingId: {BookingId}", booking.BookingId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send reminder email for BookingId: {BookingId}", booking.BookingId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CheckAndSendRemindersAsync at {Now}", now);
                }
            }
        }
    }
}