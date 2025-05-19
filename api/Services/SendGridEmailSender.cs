using api.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace api.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API Key is missing.");
                throw new InvalidOperationException("SendGrid API Key is not configured.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["SendGrid:SenderEmail"], _configuration["SendGrid:SenderName"]);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);

            try
            {
                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Error: {ErrorBody}", email, response.StatusCode, errorBody);
                    throw new InvalidOperationException($"Failed to send email: {response.StatusCode} - {errorBody}");
                }
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending email to {Email}", email);
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}