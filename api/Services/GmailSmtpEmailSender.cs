using api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace api.Services
{
    public class GmailSmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GmailSmtpEmailSender> _logger;

        public GmailSmtpEmailSender(IConfiguration config, ILogger<GmailSmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var smtpSection = _config.GetSection("GmailSmtp");
            var client = new SmtpClient
            {
                Host = smtpSection["Host"],
                Port = int.Parse(smtpSection["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
                Timeout = 10000
            };

            var from = new MailAddress(smtpSection["Username"], smtpSection["FromName"]);
            var to = new MailAddress(toEmail);

            var mail = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            try
            {
                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {Email} via Gmail SMTP", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} via Gmail SMTP", toEmail);
                throw;
            }
        }
    }
}
