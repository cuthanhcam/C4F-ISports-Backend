using api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace api.Services
{
    public class MailjetEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailjetEmailSender> _logger;

        public MailjetEmailSender(IConfiguration configuration, ILogger<MailjetEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["Mailjet:ApiKey"];
            var apiSecret = _configuration["Mailjet:ApiSecret"];
            var fromEmail = _configuration["Mailjet:FromEmail"];
            var fromName = _configuration["Mailjet:FromName"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                _logger.LogError("Mailjet API Key hoặc Secret chưa được cấu hình.");
                throw new InvalidOperationException("Mailjet API Key/Secret chưa được cấu hình.");
            }

            var client = new MailjetClient(apiKey, apiSecret);
            var request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                    { "From", new JObject {
                        { "Email", fromEmail },
                        { "Name", fromName }
                    }},
                    { "To", new JArray {
                        new JObject {
                            { "Email", email },
                            { "Name", email }
                        }
                    }},
                    { "Subject", subject },
                    { "HTMLPart", htmlMessage }
                }
            });

            try
            {
                var response = await client.PostAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = response.GetData();
                    _logger.LogError("Lỗi gửi email đến {Email}. Mã trạng thái: {StatusCode}. Lỗi: {Error}", email, response.StatusCode, error);
                    throw new InvalidOperationException($"Không thể gửi email: {response.StatusCode} - {error}");
                }

                _logger.LogInformation("Email đã gửi thành công đến {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đến {Email}", email);
                throw;
            }
        }
    }
}
